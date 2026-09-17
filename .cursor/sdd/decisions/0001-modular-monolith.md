# ADR-0001 — Modular monolith before microservices

Status: Accepted · Date: 2026-09-17

## Decision

Ship one .NET modular monolith with module folders/schemas; split deployables later only for measured scale/isolation needs.

## Consequences

Simpler ops for MVP; architecture tests enforce module mutation rules; AI worker remains a separate process for isolation without dual backends.
