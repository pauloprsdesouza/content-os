# ADR-0005 — RabbitMQ transports messages, not files

Status: Accepted · Date: 2026-09-17

## Decision

Messages carry blob hashes/refs and metadata only. Binaries and unbounded prompts never traverse the broker.

## Consequences

`IBlobStore` is mandatory for large payloads; CloudEvents stay small and schema-validated.
