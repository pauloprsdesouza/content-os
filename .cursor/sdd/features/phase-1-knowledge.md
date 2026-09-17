# Feature — Phase 1 Knowledge loop

Status: In progress (local E2E usable) · Phase: 1 · Last updated: 2026-09-17

## Outcome

Sources, immutable snapshots (blob + hash), extraction hooks, evidence, claims, claim review UI, provenance visible.

## Invariants

- Snapshot immutable; SHA-256 blob ref.
- Claim cannot approve without Evidence.
- Rejection requires reason; approval records actor/time/evidence/ETag.
- Approved claims versioned; new snapshots create impact candidates.

## Key APIs

`GET/POST /api/v1/sources` · `GET /api/v1/sources/{id}` · `GET/POST /api/v1/sources/{id}/snapshots` · `GET /api/v1/claims` (pending queue) · `GET /api/v1/claims/{id}` · `POST .../approve|reject` (If-Match) · `POST /api/v1/claims` (create-for-review, local/demo).

## Local ops

1. Start Postgres (`ops/compose`) with `POSTGRES_PASSWORD` set.
2. `dotnet run --project src/backend/ContentOS.Migrations`
3. `dotnet run --project src/backend/ContentOS.Api --launch-profile http`
4. `npm run dev` in `src/studio` (proxies `/api` → `:5231`)
5. Login `admin@contentos.local` / `ChangeMe!Admin1`

## Test plan

Domain transition tests; review UI 409/412 states. SSRF-safe ingest and impact endpoints still deferred.

## Remaining gaps

Research AI jobs, impact endpoints, Orval client, MFA enforcement, source integrity tabs, automatic extraction.
