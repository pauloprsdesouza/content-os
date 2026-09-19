# Feature — Phase 3 Product and publication

Status: Active · Phase: 3 · Last updated: 2026-09-17

## Outcome

Products, editions, explicit curriculum, PublicationPackage builder, deterministic export, manual publication confirmation.

## Invariants

- Edition references explicit approved ContentVersions (never mutable “latest”).
- Package records source versions, renderer version, asset hashes.
- Export downloads a Markdown file of the curriculum, in order. The blob remains the stored artifact. Export ≠ PublishedConfirmed.
- Package states owned by aggregate transitions.

## Key APIs

products/editions/curriculum; publication-packages list-by-edition/create/export/confirm.

## Test plan

Manifest determinism; confirm requires capability `publication.confirm`.
