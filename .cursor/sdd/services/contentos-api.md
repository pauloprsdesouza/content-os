# Service — Platform API (`ContentOS.Api`)

Status: Active · Runtime: .NET 10 ASP.NET Core + Wolverine.HTTP + Wolverine + EF Core

## Owns

Identity/sessions/MFA, all module aggregates, authorization policies, OpenAPI 3.1, Operation resources, transactional outbox/inbox, audit events, internal Knowledge Tool API for AI worker.

## Does not own

UI presentation, AI model execution, blob bytes (only metadata/hashes), RabbitMQ as business state.

## HTTP

Prefix `/api/v1`. Collections → `PageResponse<T>`. Long work → `202 OperationAccepted`. Errors → `application/problem+json`. Updates: ETag + `If-Match`. Commands: `Idempotency-Key`. camelCase JSON.

## Auth

ASP.NET Core Identity; Secure HttpOnly SameSite cookies; antiforgery on unsafe methods; MFA required for Admin; named capability policies (e.g. `knowledge.approve`, `publication.confirm`).

## Persistence

One `PlatformDbContext`; schemas: identity, knowledge, research, content, catalog, publication, commerce, learning, wolverine. Handlers use aggregate repositories + query ports — never `DbContext` directly.

## Messaging

Publish CloudEvents to `contentos.ai`; consume `backend.ai.results`. Inbox/outbox mandatory.
