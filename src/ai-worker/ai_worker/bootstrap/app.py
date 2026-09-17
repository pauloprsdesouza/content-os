import structlog

from ai_worker.settings.config import get_settings
from ai_worker.telemetry.logging import configure_logging


def main() -> None:
    settings = get_settings()
    configure_logging(settings.log_level)
    logger = structlog.get_logger("ai_worker")
    logger.info(
        "worker idle — Phase 0",
        service="contentos-ai-worker",
        environment=settings.environment,
        api_base_url=str(settings.api_base_url),
    )


if __name__ == "__main__":
    main()
