from __future__ import annotations

import asyncio
import signal

import structlog

from ai_worker.consumers.commands import AiCommandConsumer
from ai_worker.settings.config import get_settings
from ai_worker.telemetry.logging import configure_logging


async def _run() -> None:
    settings = get_settings()
    configure_logging(settings.log_level)
    logger = structlog.get_logger("ai_worker")
    consumer = AiCommandConsumer(settings)
    await consumer.start()
    logger.info(
        "worker running",
        service="contentos-ai-worker",
        environment=settings.environment,
        stub=settings.ai_stub,
        api_base_url=str(settings.api_base_url),
    )

    stop = asyncio.Event()

    def _stop(*_: object) -> None:
        stop.set()

    loop = asyncio.get_running_loop()
    for sig in (signal.SIGINT, signal.SIGTERM):
        try:
            loop.add_signal_handler(sig, _stop)
        except NotImplementedError:
            # Windows
            signal.signal(sig, lambda *_args: _stop())

    await stop.wait()
    await consumer.stop()
    logger.info("worker stopped")


def main() -> None:
    asyncio.run(_run())


if __name__ == "__main__":
    main()
