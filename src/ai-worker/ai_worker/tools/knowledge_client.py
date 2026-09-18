from __future__ import annotations

import httpx
import structlog

from ai_worker.settings.config import WorkerSettings

logger = structlog.get_logger(__name__)


class KnowledgeToolClient:
    """Internal API facade — snapshot metadata and bounded text excerpts by hash."""

    def __init__(self, settings: WorkerSettings) -> None:
        self._settings = settings

    async def get_snapshot_by_hash(self, content_hash: str) -> dict | None:
        key = (
            self._settings.worker_api_key.get_secret_value()
            if self._settings.worker_api_key is not None
            else None
        )
        if not key:
            logger.warning("worker api key missing; skip snapshot lookup")
            return None

        url = f"{str(self._settings.api_base_url).rstrip('/')}/api/v1/internal/knowledge/snapshots/by-hash/{content_hash}"
        async with httpx.AsyncClient(timeout=10.0) as client:
            response = await client.get(
                url,
                headers={"X-ContentOS-Worker-Key": key},
            )
            if response.status_code == 404:
                return None
            response.raise_for_status()
            return response.json()

    async def get_excerpt(self, content_hash: str) -> str | None:
        key = (
            self._settings.worker_api_key.get_secret_value()
            if self._settings.worker_api_key is not None
            else None
        )
        if not key:
            logger.warning("worker api key missing; skip excerpt lookup")
            return None

        url = (
            f"{str(self._settings.api_base_url).rstrip('/')}"
            f"/api/v1/internal/knowledge/snapshots/by-hash/{content_hash}/excerpt"
        )
        async with httpx.AsyncClient(timeout=10.0) as client:
            response = await client.get(
                url,
                headers={"X-ContentOS-Worker-Key": key},
            )
            if response.status_code in {401, 404}:
                return None
            response.raise_for_status()
            text = response.json().get("text")
            if not isinstance(text, str) or not text.strip():
                return None
            return text
