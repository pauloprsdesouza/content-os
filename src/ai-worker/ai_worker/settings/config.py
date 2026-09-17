from functools import lru_cache

from pydantic import AnyHttpUrl, AmqpDsn, Field, SecretStr
from pydantic_settings import BaseSettings, SettingsConfigDict


class WorkerSettings(BaseSettings):
    model_config = SettingsConfigDict(
        env_prefix="CONTENT_OS_",
        env_file=".env",
        env_file_encoding="utf-8",
        extra="ignore",
    )

    rabbitmq_url: AmqpDsn = "amqp://contentos:contentos@localhost:5672/"
    api_base_url: AnyHttpUrl = "http://localhost:5080"
    environment: str = "development"
    log_level: str = "INFO"
    ai_stub: bool = True
    litellm_api_key: SecretStr | None = None
    litellm_model_alias: str = "contentos/author"
    requests_exchange: str = "contentos.ai"
    requests_queue: str = "ai.worker.commands"
    results_routing_key: str = "backend.ai.results"
    results_exchange: str = "contentos.ai"
    worker_api_key: SecretStr | None = Field(
        default=None,
        description="Shared key for internal Knowledge tool API",
    )
    ledger_path: str = ".contentos-ai-ledger.jsonl"


@lru_cache
def get_settings() -> WorkerSettings:
    return WorkerSettings()
