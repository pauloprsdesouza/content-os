# Service — AI Worker (`src/ai-worker`)

Status: Active · Runtime: Python · Strands Agents · LiteLLM SDK · aio-pika · Pydantic · httpx · Tenacity

## Owns

Temporary job execution: research, content generate/review, document normalize. Prompt templates (versioned assets), tool allowlists, token/cost budgets, result CloudEvents.

## MUST NOT

- Hold PostgreSQL credentials or write domain DB.
- Own workflow/authorization/business truth.
- Put binaries or unbounded prompts on RabbitMQ.
- Treat document text as trusted instructions.
- Apply model output as a domain command (validate → publish typed result only).

## Access

Knowledge via restricted internal API tools with workload identity. Blob open by authorized hash only.

## Packages

`bootstrap`, `contracts`, `consumers`, `agents`, `tools`, `model_gateway`, `extraction`, `rendering`, `telemetry`, `settings`.
