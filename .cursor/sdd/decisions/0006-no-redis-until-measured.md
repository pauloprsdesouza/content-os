# ADR-0006 — No Redis until measured need

Status: Accepted · Date: 2026-09-17

## Decision

Exclude Redis from MVP. Postgres + RabbitMQ + filesystem cover state, messaging, and blobs.

## Consequences

Revisit only with measured cache/session/queue pressure evidence.
