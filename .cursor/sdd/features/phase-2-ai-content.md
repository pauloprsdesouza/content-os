# Feature — Phase 2 AI content loop

Status: Active · Phase: 2 · Last updated: 2026-09-17

## Outcome

Research jobs, author/reviewer agents, model gateway aliases, ContentUnit/ContentVersion lifecycle, Operation monitoring, human gates.

## Invariants

- Worker reports attempt results; never owns durable workflow state.
- Only Wolverine schedules durable retries.
- ContentVersion: Draft → … → PendingHumanApproval → Approved|ChangesRequested; approved immutable.
- Model output validated; never applied as raw domain command.

## Messaging

Commands: `ai.research`, `ai.content.generate`, `ai.content.review`, `ai.documents.normalize`. Results: `backend.ai.results`.

## Test plan

Duplicate delivery; schema validation; Operation 202 polling; budget/timeout enforcement.
