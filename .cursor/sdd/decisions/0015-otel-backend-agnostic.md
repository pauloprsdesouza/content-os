# ADR-0015 — Observability is OTLP backend-agnostic

Status: Accepted · Date: 2026-09-17

## Decision

Instrument with OpenTelemetry → OTLP. Swap collectors/backends without app code changes.

## Consequences

Serilog/structlog + OTEL; no vendor SDKs in domain paths; propagate W3C trace context on HTTP and CloudEvents.
