# Feature — Phase 4 Commerce and outcomes

Status: Active · Phase: 4 · Last updated: 2026-09-17

## Outcome

Kiwify webhook inbox, reconciliation runs, purchases, learners, enrollments, capstones, evaluations, outcome summary.

## Invariants

- Webhook = signal only; Purchase authoritative after reconciliation.
- Provider + external_id unique.
- Outcomes formulas live in backend DTO — not duplicated in React.

## Key APIs

purchases, commerce/reconciliation-runs, outcomes/summary, learning flows.

## Test plan

Signature verify (when available); duplicate webhook; reconcile idempotency; UI never labels webhook as confirmed sale.
