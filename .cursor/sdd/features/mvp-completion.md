# Feature — MVP completion

Status: Active · Baseline after Phase 0: `582c75d` · Updated: 2026-09-18

Phase 0 is the production-readiness commit already on `main`. Do not revert it.

## This slice (Phase 1 start + Studio account)

- `contracts/http/openapi.json` is the live `/openapi/v1.json` export. `servers.url` is `/` so the file is not host-specific.
- Event schemas match the outbox wire types `ai.research`, `ai.content.generate`, `ai.content.review`. Versioned `contentos.*.v1` names wait until the worker binds both.
- `ops/contracts/check.ps1` fails on a stub OpenAPI or, when the API is up, on drift.
- Studio account menu opens a read-only profile. Password, MFA, and session lists stay hidden.
- Shared API client maps 403/404/409/412/422/429/5xx. 401 still clears CSRF and returns to login.

## Messaging slice

- AI results are remembered in `wolverine.inbox_messages`. A repeated `id` is acknowledged and does not rerun the handler.
- Invalid or unknown results go to `backend.ai.results.dlq` after one redelivery. The body is not logged.
- Findings now carry optional `sourceSnapshotId`, `locator`, `findingId`.
- Compose can build API, migrations, worker, and Studio. Caddy no longer targets `host.docker.internal:5080`.
- `ops/release/gate.ps1` runs tests, contracts, and a secret scan. Backup refuses to run without `pg_dump` and a connection variable that is never printed.

## Still open

- Orval, Testcontainers, Playwright, coverage gate, SBOM, and a real restore drill.
- Wolverine RabbitMQ transport is not the wire format. The worker still consumes raw CloudEvents, so the outbox dispatcher stays.
- Doppler `prd` still has placeholders. Do not deploy.
