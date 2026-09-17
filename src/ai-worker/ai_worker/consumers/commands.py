from __future__ import annotations

import json
from typing import Any

import aio_pika
import structlog

from ai_worker.agents.stub_agent import StubAgent
from ai_worker.consumers.ledger import ProcessingLedger
from ai_worker.contracts.messages import (
    CloudEvent,
    ContentGenerateCommandData,
    ContentReviewCommandData,
    JobFailedData,
    ResearchCommandData,
)
from ai_worker.model_gateway.gateway import ModelGateway
from ai_worker.settings.config import WorkerSettings

logger = structlog.get_logger(__name__)

COMMAND_TYPES = {
    "ai.research",
    "ai.content.generate",
    "ai.content.review",
}


class AiCommandConsumer:
    def __init__(self, settings: WorkerSettings) -> None:
        self._settings = settings
        self._ledger = ProcessingLedger(settings.ledger_path)
        self._gateway = ModelGateway(settings)
        self._agent = StubAgent(self._gateway)
        self._connection: aio_pika.RobustConnection | None = None
        self._channel: aio_pika.Channel | None = None

    async def start(self) -> None:
        self._connection = await aio_pika.connect_robust(str(self._settings.rabbitmq_url))
        self._channel = await self._connection.channel()
        await self._channel.set_qos(prefetch_count=5)

        exchange = await self._channel.declare_exchange(
            self._settings.requests_exchange,
            aio_pika.ExchangeType.TOPIC,
            durable=True,
        )
        queue = await self._channel.declare_queue(
            self._settings.requests_queue,
            durable=True,
        )
        for routing_key in COMMAND_TYPES:
            await queue.bind(exchange, routing_key=routing_key)

        # Ensure results can be published to the same exchange with a bound queue on API side.
        await self._channel.declare_queue(
            "backend.ai.results",
            durable=True,
        )
        results_queue = await self._channel.declare_queue(
            "backend.ai.results",
            durable=True,
        )
        await results_queue.bind(exchange, routing_key="ai.#")

        await queue.consume(self._on_message)
        logger.info(
            "ai consumer started",
            queue=self._settings.requests_queue,
            stub=self._gateway.use_stub,
        )

    async def stop(self) -> None:
        if self._connection is not None:
            await self._connection.close()

    async def _on_message(self, message: aio_pika.IncomingMessage) -> None:
        async with message.process(requeue=False):
            payload = json.loads(message.body.decode("utf-8"))
            event_type = payload.get("type")
            idempotency_key = payload.get("idempotencykey") or payload.get("id") or ""
            if not event_type:
                logger.warning("message missing type")
                return
            if self._ledger.already_processed(idempotency_key):
                logger.info("duplicate skipped", idempotency_key=idempotency_key)
                return

            try:
                result_event = await self._handle(payload)
            except Exception as exc:  # noqa: BLE001 - publish typed failure
                logger.exception("job failed", type=event_type)
                result_event = self._failure_event(payload, event_type, str(exc))

            await self._publish_result(result_event)
            self._ledger.mark_processed(idempotency_key, event_type)

    async def _handle(self, payload: dict[str, Any]) -> CloudEvent:
        event_type = payload["type"]
        data = payload.get("data") or {}
        correlation = payload.get("correlationid")
        subject = payload.get("subject") or "unknown"
        idempotency_key = payload.get("idempotencykey") or payload["id"]

        if event_type == "ai.research":
            command = ResearchCommandData.model_validate(data)
            completed = await self._agent.research(command)
            return CloudEvent(
                type="ai.research.completed",
                subject=subject,
                correlationid=correlation,
                idempotencykey=f"result:{idempotency_key}",
                data=completed.model_dump(by_alias=True, mode="json"),
            )

        if event_type == "ai.content.generate":
            command = ContentGenerateCommandData.model_validate(data)
            completed = await self._agent.generate(command)
            return CloudEvent(
                type="ai.content.generate.completed",
                subject=subject,
                correlationid=correlation,
                idempotencykey=f"result:{idempotency_key}",
                data=completed.model_dump(by_alias=True, mode="json"),
            )

        if event_type == "ai.content.review":
            command = ContentReviewCommandData.model_validate(data)
            completed = await self._agent.review(command)
            return CloudEvent(
                type="ai.content.review.completed",
                subject=subject,
                correlationid=correlation,
                idempotencykey=f"result:{idempotency_key}",
                data=completed.model_dump(by_alias=True, mode="json"),
            )

        raise ValueError(f"Unsupported command type: {event_type}")

    def _failure_event(
        self, payload: dict[str, Any], failed_type: str, error: str
    ) -> CloudEvent:
        data = payload.get("data") or {}
        failed = JobFailedData(
            operation_id=data.get("operationId") or data.get("operation_id"),
            error_message=error,
            failed_type=failed_type,
            research_job_id=data.get("researchJobId") or data.get("research_job_id"),
            content_version_id=data.get("contentVersionId")
            or data.get("content_version_id"),
        )
        return CloudEvent(
            type="ai.job.failed",
            subject=payload.get("subject") or "unknown",
            correlationid=payload.get("correlationid"),
            idempotencykey=f"result:fail:{payload.get('idempotencykey') or payload.get('id')}",
            data=failed.model_dump(by_alias=True, mode="json", exclude_none=True),
        )

    async def _publish_result(self, event: CloudEvent) -> None:
        assert self._channel is not None
        exchange = await self._channel.declare_exchange(
            self._settings.results_exchange,
            aio_pika.ExchangeType.TOPIC,
            durable=True,
        )
        body = event.model_dump_json().encode("utf-8")
        await exchange.publish(
            aio_pika.Message(
                body=body,
                content_type="application/cloudevents+json",
                delivery_mode=aio_pika.DeliveryMode.PERSISTENT,
                message_id=event.id,
                correlation_id=event.correlationid,
                type=event.type,
            ),
            routing_key=event.type,
        )
        logger.info("published result", type=event.type, subject=event.subject)
