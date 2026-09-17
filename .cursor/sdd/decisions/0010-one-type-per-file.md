# ADR-0010 — One hand-written C# type per file

Status: Accepted · Date: 2026-09-17

## Decision

Every hand-written type lives alone in a file named after the type. Analyzer + architecture tests enforce.

## Consequences

No Dtos.cs / Models.cs bundles; nested request/response types prohibited; generated files exempt by marker/path.
