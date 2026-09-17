# ADR-0007 — LiteLLM SDK in-process; no proxy in MVP

Status: Accepted · Date: 2026-09-17

## Decision

Worker uses LiteLLM SDK directly with typed alias Options. No LiteLLM Proxy / FastAPI gateway in MVP.

## Consequences

Simpler ops; alias/fallback/residency policy in config; proxy may return as shared infra later.
