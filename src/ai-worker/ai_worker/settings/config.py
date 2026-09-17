from functools import lru_cache

from pydantic import AnyHttpUrl, AmqpDsn
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


@lru_cache
def get_settings() -> WorkerSettings:
    return WorkerSettings()
