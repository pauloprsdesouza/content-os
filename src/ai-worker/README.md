# Content OS AI Worker

Consumes `ai.research`, `ai.content.generate`, and `ai.content.review` CloudEvents
from RabbitMQ, runs a stub or LiteLLM-backed agent path, and publishes typed
result events. **No PostgreSQL credentials.**

## Stub mode (local E2E)

Stub mode is the default when `CONTENT_OS_AI_STUB=true` **or** when
`CONTENT_OS_LITELLM_API_KEY` is unset.

Prefer Doppler (Homelab RabbitMQ + worker key):

```powershell
.\ops\doppler\run-ai-worker.ps1
```

Manual stub (Compose localhost only):

```powershell
Set-Location src/ai-worker
python -m venv .venv
.\.venv\Scripts\python -m pip install -e .
$env:CONTENT_OS_AI_STUB = "true"
$env:CONTENT_OS_RABBITMQ_URL = "amqp://contentos:change-me-local-only@127.0.0.1:5672/"
$env:CONTENT_OS_API_BASE_URL = "http://localhost:5231"
$env:CONTENT_OS_WORKER_API_KEY = "local-dev-worker-key"
.\.venv\Scripts\python -m ai_worker.bootstrap.app
```

Optional real model path:

```powershell
$env:CONTENT_OS_AI_STUB = "false"
$env:CONTENT_OS_LITELLM_API_KEY = "<key>"
$env:CONTENT_OS_LITELLM_MODEL_ALIAS = "gpt-4o-mini"
```

## Messaging

| Direction | Exchange | Routing / queue |
|-----------|----------|-----------------|
| Commands in | `contentos.ai` (topic) | `ai.research`, `ai.content.generate`, `ai.content.review` → queue `ai.worker.commands` |
| Results out | `contentos.ai` | result types (`ai.research.completed`, …); API binds `backend.ai.results` to `ai.#` |

Idempotency: append-only ledger file (default `.contentos-ai-ledger.jsonl`).

## Internal Knowledge tool

`GET /api/v1/internal/knowledge/snapshots/by-hash/{hash}` with header
`X-ContentOS-Worker-Key` — metadata only, no DB access from the worker.
