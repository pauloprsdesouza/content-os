# ADR-0008 — Filesystem behind IBlobStore

Status: Accepted · Date: 2026-09-17

## Decision

Default blob implementation writes temp then atomic rename to SHA-256 path. Domain stores hash/length/media type/purpose — never machine paths.

## Consequences

Object storage is a future adapter behind the same port.
