# ADR-0013 — Kiwify webhook is a signal; reconciliation is authoritative

Status: Accepted · Date: 2026-09-17

## Decision

Webhook intake creates SignalReceived work. Purchase becomes Confirmed (or other terminal) only after provider API reconciliation.

## Consequences

UI must not label webhook receipt as a confirmed sale; provider+external_id uniqueness; verify signatures when available.
