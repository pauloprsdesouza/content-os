from __future__ import annotations

import structlog

from ai_worker.settings.config import WorkerSettings

logger = structlog.get_logger(__name__)


class ModelGateway:
    """LiteLLM alias boundary with deterministic stub fallback."""

    def __init__(self, settings: WorkerSettings) -> None:
        self._settings = settings
        key = (
            settings.litellm_api_key.get_secret_value()
            if settings.litellm_api_key is not None
            else None
        )
        self._use_stub = settings.ai_stub or not key

    @property
    def use_stub(self) -> bool:
        return self._use_stub

    async def complete(self, *, system: str, prompt: str, stub_fallback: str) -> str:
        if self._use_stub:
            logger.info("model stub path", alias=self._settings.litellm_model_alias)
            return stub_fallback

        try:
            from litellm import acompletion
        except ImportError:
            logger.warning("litellm not installed; falling back to stub")
            return stub_fallback

        key = self._settings.litellm_api_key
        assert key is not None
        response = await acompletion(
            model=self._settings.litellm_model_alias,
            messages=[
                {"role": "system", "content": system},
                {"role": "user", "content": prompt},
            ],
            api_key=key.get_secret_value(),
        )
        content = response["choices"][0]["message"]["content"]
        return str(content)
