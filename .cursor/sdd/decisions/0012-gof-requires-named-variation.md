# ADR-0012 — GoF patterns require named variation

Status: Accepted · Date: 2026-09-17

## Decision

Use GoF only when isolating a real variation or removing owned duplication. Name the changing dimension, owner, and payoff.

## Consequences

No pattern-for-pattern's-sake; single-implementation interfaces need a boundary/test seam or expected variation.
