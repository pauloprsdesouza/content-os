# Quality attributes

| Attribute | Target |
|---|---|
| Maintainability | No forbidden deps; one type/file; Domain/App coverage ≥90% lines / ≥85% branches |
| Reliability | At-least-once + idempotent consumers; TX outbox; RPO ≤24h / RTO ≤4h |
| Security | Cookie + antiforgery; MFA Admin; SSRF-safe ingest; AI tools allowlisted |
| Performance | Read p95 <500ms; command p95 <800ms (excl. async); queues via projections |
| Observability | OTEL end-to-end; audit ledger separate from logs |

Also: `engineering.md`, `rules/00-core-engineering.mdc`, `rules/11-design-qualities.mdc`, `rules/41-observability-security.mdc`.
