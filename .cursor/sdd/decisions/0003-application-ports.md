# ADR-0003 — Application ports instead of DbContext in handlers

Status: Accepted · Date: 2026-09-17

## Decision

Handlers depend on aggregate repositories and purpose-specific query ports. Infrastructure implements them. No `IUnitOfWork`.

## Consequences

Clean Architecture preserved; EF + Wolverine middleware owns commit.
