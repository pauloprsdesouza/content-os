# ADR-0004 — Wolverine owns durable orchestration; Strands owns AI execution

Status: Accepted · Date: 2026-09-17

## Decision

Wolverine: workflows, retries, inbox/outbox, sagas. Strands: bounded agent/tool/model work inside a job. Python never owns durable business state.

## Consequences

Worker publishes typed results; .NET applies idempotently.
