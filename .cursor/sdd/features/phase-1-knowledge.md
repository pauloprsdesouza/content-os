# Feature — Phase 1 Knowledge loop

Status: Active · Phase: 1 · Last updated: 2026-09-17

## Outcome

Sources, immutable snapshots (blob + hash), extraction hooks, evidence, claims, claim review UI, provenance visible.

## Invariants

- Snapshot immutable; SHA-256 blob ref.
- Claim cannot approve without Evidence.
- Rejection requires reason; approval records actor/time/evidence/ETag.
- Approved claims versioned; new snapshots create impact candidates.

## Key APIs

`/sources`, `/snapshots`, `/claims/{id}`, approve/reject, impact endpoints.

## Test plan

Domain transition tests; SSRF-safe ingest tests; review UI 412/409 states.
