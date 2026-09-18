from __future__ import annotations

import re
from uuid import uuid5

from ai_worker.contracts.messages import (
    ContentGenerateCommandData,
    ContentGenerateCompletedData,
    ContentReviewCommandData,
    ContentReviewCompletedData,
    ResearchCommandData,
    ResearchCompletedData,
    ResearchFindingResult,
)
from ai_worker.model_gateway.gateway import ModelGateway
from ai_worker.tools.knowledge_client import KnowledgeToolClient


class StubAgent:
    def __init__(self, gateway: ModelGateway, knowledge: KnowledgeToolClient) -> None:
        self._gateway = gateway
        self._knowledge = knowledge

    async def research(self, command: ResearchCommandData) -> ResearchCompletedData:
        excerpt = await self._knowledge.get_excerpt(command.content_hash)
        if not excerpt:
            raise RuntimeError(
                "O snapshot não tem texto legível. PDF ainda não é extraído pela pesquisa."
            )

        fallback = "\n".join(self._sentences(excerpt))
        method = "snapshot-excerpt"
        text = fallback
        if not self._gateway.use_stub:
            try:
                text = await self._gateway.complete(
                    system=(
                        "Extract short factual claims grounded only in the supplied excerpt. "
                        "One claim per line. Do not browse, and do not mention a URL."
                    ),
                    prompt=(
                        f"Topic: {command.topic}\n"
                        f"Scope: {command.scope_notes or 'general'}\n"
                        f"Excerpt:\n{excerpt}"
                    ),
                    stub_fallback=fallback,
                )
                method = "litellm"
            except Exception:
                text = fallback
                method = "snapshot-excerpt"

        lines = [line.strip("- ").strip() for line in text.splitlines() if line.strip()]
        if not lines:
            lines = self._sentences(excerpt)
        findings = [
            ResearchFindingResult(
                statement=line[:500],
                confidence=0.72,
                source_snapshot_id=command.source_snapshot_id,
                locator=self._locator(line, excerpt, index),
                extraction_method=method,
                finding_id=uuid5(command.research_job_id, f"finding:{index}"),
            )
            for index, line in enumerate(lines[:3])
        ]
        return ResearchCompletedData(
            research_job_id=command.research_job_id,
            operation_id=command.operation_id,
            findings=findings,
        )

    @staticmethod
    def _sentences(excerpt: str) -> list[str]:
        parts = re.split(r"(?<=[.!?])\s+", excerpt)
        sentences = [part.strip() for part in parts if len(part.strip()) >= 40]
        if sentences:
            return sentences[:3]
        compact = " ".join(excerpt.split())
        return [compact[:500]] if compact else []

    @staticmethod
    def _locator(statement: str, excerpt: str, index: int) -> str:
        position = excerpt.find(statement[:120])
        return f"offset:{position if position >= 0 else index}"

    async def generate(
        self, command: ContentGenerateCommandData
    ) -> ContentGenerateCompletedData:
        title = command.title or "Untitled unit"
        brief = command.brief or "No brief provided"
        body = await self._gateway.complete(
            system="You write concise markdown content drafts.",
            prompt=f"Title: {title}\nBrief: {brief}\nWrite a short markdown article.",
            stub_fallback=(
                f"# {title}\n\n"
                f"{brief}\n\n"
                "## Draft\n\n"
                "This is a deterministic stub ContentVersion body for local MVP E2E.\n\n"
                "- Point one\n- Point two\n"
            ),
        )
        return ContentGenerateCompletedData(
            content_version_id=command.content_version_id,
            operation_id=command.operation_id,
            body_markdown=body.strip(),
            request_review_after_generate=command.request_review_after_generate,
        )

    async def review(
        self, command: ContentReviewCommandData
    ) -> ContentReviewCompletedData:
        notes = await self._gateway.complete(
            system="You review draft content briefly.",
            prompt=f"Review:\n{command.body_markdown or ''}",
            stub_fallback="Stub agent review: structure is clear; ready for human approval.",
        )
        return ContentReviewCompletedData(
            content_version_id=command.content_version_id,
            operation_id=command.operation_id,
            agent_review_notes=notes.strip(),
        )
