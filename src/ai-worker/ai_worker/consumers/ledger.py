from __future__ import annotations

import json
from pathlib import Path

import structlog

logger = structlog.get_logger(__name__)


class ProcessingLedger:
    """Append-only idempotency ledger (no Postgres)."""

    def __init__(self, path: str) -> None:
        self._path = Path(path)
        self._seen: set[str] = set()
        self._load()

    def _load(self) -> None:
        if not self._path.exists():
            return
        for line in self._path.read_text(encoding="utf-8").splitlines():
            if not line.strip():
                continue
            try:
                payload = json.loads(line)
                key = payload.get("idempotencyKey")
                if isinstance(key, str):
                    self._seen.add(key)
            except json.JSONDecodeError:
                continue

    def already_processed(self, idempotency_key: str) -> bool:
        return idempotency_key in self._seen

    def mark_processed(self, idempotency_key: str, message_type: str) -> None:
        if idempotency_key in self._seen:
            return
        self._seen.add(idempotency_key)
        self._path.parent.mkdir(parents=True, exist_ok=True)
        with self._path.open("a", encoding="utf-8") as handle:
            handle.write(
                json.dumps(
                    {"idempotencyKey": idempotency_key, "type": message_type},
                    ensure_ascii=True,
                )
                + "\n"
            )
        logger.info("ledger marked", idempotency_key=idempotency_key, type=message_type)
