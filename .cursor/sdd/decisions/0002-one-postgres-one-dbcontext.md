# ADR-0002 — One PostgreSQL + one PlatformDbContext

Status: Accepted · Date: 2026-09-17

## Decision

Single database; schemas per module + `wolverine`; one `PlatformDbContext` for mappings and transactions.

## Consequences

Cross-schema FKs need ADR and are limited to stable references; module tables mutated only by owning module.
