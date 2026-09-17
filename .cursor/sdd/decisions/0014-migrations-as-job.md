# ADR-0014 — Migrations are a deployment job

Status: Accepted · Date: 2026-09-17

## Decision

`ContentOS.Migrations` runs independently with advisory locking. Production API MUST NOT call `Database.Migrate()` at startup.

## Consequences

Compose/deploy order: migrate → API/worker; every migration documents forward verify + rollback/roll-forward.
