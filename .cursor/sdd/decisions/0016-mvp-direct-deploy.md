# ADR-0016 — MVP direct deploy; CI/CD deferred

Status: Accepted · Date: 2026-09-17

## Decision

First usable MVP deploys via Docker Compose directly to the target server (Hostinger VPS and/or Homelab). Full CI/CD gate pipeline from the long SDD is deferred.

## Consequences

Still run architecture tests and critical checks locally before promote. No branch-protection requirement yet. Revisit CI/CD after the critical path is stable in production-like infra.
