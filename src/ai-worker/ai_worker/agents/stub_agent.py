from __future__ import annotations

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


class StubAgent:
    def __init__(self, gateway: ModelGateway) -> None:
        self._gateway = gateway

    async def research(self, command: ResearchCommandData) -> ResearchCompletedData:
        text = await self._gateway.complete(
            system="You extract short factual claims for Content OS research.",
            prompt=(
                f"Topic: {command.topic}\n"
                f"Scope: {command.scope_notes or 'general'}\n"
                "Return 2 short claims."
            ),
            stub_fallback=(
                f"Claim about {command.topic}: provenance matters.\n"
                f"Claim about {command.topic}: human gates remain required."
            ),
        )
        lines = [line.strip("- ").strip() for line in text.splitlines() if line.strip()]
        findings = [
            ResearchFindingResult(
                statement=line[:500],
                confidence=0.72,
                source_snapshot_id=command.source_snapshot_id,
                locator=f"stub:{index + 1}",
                extraction_method="stub-agent",
                finding_id=uuid5(command.research_job_id, f"finding:{index}"),
            )
            for index, line in enumerate(lines[:3])
        ]
        if not findings:
            findings = [
                ResearchFindingResult(
                    statement=f"Stub finding for topic '{command.topic}'.",
                    confidence=0.7,
                    source_snapshot_id=command.source_snapshot_id,
                    locator="stub:1",
                    extraction_method="stub-agent",
                    finding_id=uuid5(command.research_job_id, "finding:0"),
                )
            ]
        return ResearchCompletedData(
            research_job_id=command.research_job_id,
            operation_id=command.operation_id,
            findings=findings,
        )

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
