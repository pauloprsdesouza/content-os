# Content OS AI Worker

Phase 0 bootstrap for the temporary AI job runner. It loads environment-backed
settings and emits a structured idle log. It never receives database credentials
and does not own domain state.

## Run locally

```bash
python -m venv .venv
python -m pip install -e .
python -m ai_worker.bootstrap.app
```

Configuration uses the `CONTENT_OS_` prefix:

- `CONTENT_OS_RABBITMQ_URL` (default: `amqp://contentos:contentos@localhost:5672/`)
- `CONTENT_OS_API_BASE_URL` (default: `http://localhost:5080`)
- `CONTENT_OS_ENVIRONMENT` (default: `development`)

`aio-pika`, Strands Agents, LiteLLM, HTTP tooling, consumers, and OpenTelemetry
will be introduced with executable AI jobs in a later phase.
