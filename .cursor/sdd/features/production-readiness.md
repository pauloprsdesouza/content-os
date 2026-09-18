# Feature — Production readiness

Status: Active · Baseline: `0b7442a` · Updated: 2026-09-18

## Shipped in this slice

- Durable outbox: AI commands are inserted in the same `SaveChanges` as the aggregate. Dispatcher publishes to RabbitMQ and retries. Broker down does not drop the command.
- Research findings promote to Knowledge claims when snapshot + locator exist; otherwise counted as needs-evidence. Origin is unique.
- Kiwify HTTP reconciler reads `GET /v1/sales/{id}` and requires `Commerce:ProductMappings` (`ext=product|edition`). Unmapped products are not confirmed.
- Dashboard summary is a database projection. Studio no longer shows static counters.
- Production startup refuses placeholders, stub commerce, and development seed.

## Still open

- Wolverine-native inbox/dead-letter (current result path still uses the RabbitMQ consumer).
- Controlled live Kiwify certification and Hostinger `REPLACE_ME_*` fill.
- Release gate SBOM, backup restore drill, and exercised rollback.

## Run

Apply migrations, then `.\ops\doppler\run-api.ps1`. Preflight names only: `.\ops\release\preflight.ps1`.
