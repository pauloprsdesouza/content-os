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
    TopicLabelCommandData,
    TopicLabelCompletedData,
    TopicLabelResult,
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
                "O snapshot não tem texto legível."
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
        title = command.title or "Conteúdo"
        brief = command.brief or ""
        mold = command.format_mold or "Escreva um rascunho em markdown."
        excerpts: list[str] = []
        for content_hash in command.citation_content_hashes:
            if "://" in content_hash:
                continue
            excerpt = await self._knowledge.get_excerpt(content_hash)
            if excerpt:
                excerpts.append(f"Hash {content_hash}:\n{excerpt}")
        excerpt_block = "\n\n".join(excerpts) if excerpts else "Nenhum material citado."
        body = await self._gateway.complete(
            system=(
                "Você escreve rascunhos em português do Brasil no molde pedido. "
                "Use só os trechos fornecidos. Não navegue, não invente fonte e não inclua URL."
            ),
            prompt=(
                f"Formato: {command.format or 'artigo'}\n"
                f"Molde:\n{mold}\n"
                f"Título: {title}\n"
                f"Brief: {brief}\n"
                f"Trechos:\n{excerpt_block}"
            ),
            stub_fallback=(
                f"# {title}\n\n"
                f"Formato: {command.format or 'artigo'}\n\n"
                f"{mold}\n\n"
                f"{brief}\n\n"
                f"{excerpt_block}\n"
            ),
        )
        return ContentGenerateCompletedData(
            content_version_id=command.content_version_id,
            operation_id=command.operation_id,
            body_markdown=body.strip(),
            request_review_after_generate=command.request_review_after_generate,
        )

    async def label_topics(
        self, command: TopicLabelCommandData
    ) -> TopicLabelCompletedData:
        known = {work.work_id for work in command.works if work.work_id and "://" not in work.work_id}
        if not command.works:
            topics: list[TopicLabelResult] = []
        else:
            grouped: dict[str, list[str]] = {}
            for work in command.works:
                if work.work_id not in known:
                    continue
                key = (work.topic_name or "Sem tópico").strip() or "Sem tópico"
                grouped.setdefault(key, []).append(work.work_id)
            fallback = [
                TopicLabelResult(
                    label=label,
                    rationale="Agrupado pelos works já recuperados.",
                    work_ids=work_ids,
                )
                for label, work_ids in grouped.items()
                if work_ids
            ]
            raw = await self._gateway.complete(
                system=(
                    "Rotule apenas os works fornecidos. Responda JSON "
                    "{\"topics\":[{\"label\":\"\",\"rationale\":\"\",\"workIds\":[\"W123\"]}]}. "
                    "Cada tópico precisa de workIds que já estão na lista. "
                    "Se não houver works, devolva {\"topics\":[]}. Não invente obra e não inclua URL."
                ),
                prompt=self._label_prompt(command),
                stub_fallback=self._topics_json(fallback),
            )
            topics = self._parse_topics(raw, known) or fallback
        return TopicLabelCompletedData(
            discovery_id=command.discovery_id,
            operation_id=command.operation_id,
            topics=topics,
        )

    @staticmethod
    def _label_prompt(command: TopicLabelCommandData) -> str:
        lines = []
        for work in command.works:
            lines.append(
                f"workId={work.work_id}\ntitle={work.title}\ntopic={work.topic_name or ''}\n"
                f"abstract={work.abstract_text[:800]}"
            )
        return "\n\n".join(lines)

    @staticmethod
    def _topics_json(topics: list[TopicLabelResult]) -> str:
        import json

        return json.dumps(
            {
                "topics": [
                    {
                        "label": topic.label,
                        "rationale": topic.rationale,
                        "workIds": topic.work_ids,
                    }
                    for topic in topics
                ]
            },
            ensure_ascii=False,
        )

    @staticmethod
    def _parse_topics(raw: str, known: set[str]) -> list[TopicLabelResult]:
        import json

        start = raw.find("{")
        end = raw.rfind("}")
        if start < 0 or end <= start:
            return []
        try:
            payload = json.loads(raw[start : end + 1])
        except json.JSONDecodeError:
            return []
        parsed: list[TopicLabelResult] = []
        for item in payload.get("topics") or []:
            if not isinstance(item, dict):
                continue
            work_ids = [
                work_id
                for work_id in item.get("workIds") or []
                if isinstance(work_id, str) and work_id in known
            ]
            label = str(item.get("label") or "").strip()
            if not label or not work_ids:
                continue
            parsed.append(
                TopicLabelResult(
                    label=label[:300],
                    rationale=str(item.get("rationale") or "").strip()[:500],
                    work_ids=work_ids,
                )
            )
        return parsed

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
