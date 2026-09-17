# Content OS — Software Design Document


> **Content Studio, Platform Backend and AI Worker**


Version 1.0 | Architecture baseline 1.1 | Approved for implementation


Date: 17 September 2026 | SDD language: English | Product UI: pt-BR


Authoritative design reference: https://www.figma.com/design/ws9h9RE7NGrE9hVbwqRnxY


This document is an implementable, testable design for Content OS. MUST, MUST NOT, SHOULD and MAY are normative. It establishes Clean Architecture, Clean Code and SOLID as enforced rules; mandates one hand-written C# type per .cs file; and permits GoF patterns only when they isolate a real variation or remove owned duplication.


## 1. Executive summary


Content OS is an internal content operations platform that turns trusted sources into researched claims, approved learning content, product editions, publication packages, sales records and measurable learner outcomes.


The first delivery is a modular monolith in .NET 10, a React 19 administration application and an isolated Python AI worker connected through RabbitMQ. PostgreSQL is the system of record. Immutable snapshots and content-addressed blobs preserve provenance. Wolverine owns durable business workflows. Strands Agents performs bounded AI work. Python has no domain database credentials and is not a second backend.


The architecture optimizes for auditability, human approval, replaceable integrations, reuse and operational simplicity. External providers are hidden behind narrow ports. HTTP and event contracts are generated and verified. Failures are observable, safe to retry and correlated end-to-end.


Clean Architecture is enforced physically and automatically. Domain and Application cannot depend on Infrastructure. Each C# class, record, struct, enum, interface or delegate lives in its own correctly named file. Architecture tests and analyzers fail the build when these rules are violated.


A design pattern is not a deliverable by itself. Every abstraction must name the changing dimension it isolates, have a clear owner and remain smaller than the duplication it replaces.


## 2. Goals, scope and exclusions


### 2.1 Product goals


- Build a traceable path from source evidence to published product and learner outcome.
- Keep every generated claim and content version reviewable by a human.
- Make integrations replaceable without leaking provider details into business code.
- Generate frontend clients from OpenAPI rather than writing HTTP calls by hand.
- Support self-hosted deployment with a small operational footprint.
- Make failures recoverable, observable and idempotent.
- Avoid hardcode, conceptual duplication and premature abstraction.


### 2.2 MVP scope


- Identity, cookie authentication, MFA/TOTP, Admin and Reviewer capabilities.
- Sources, immutable snapshots, extraction, evidence, claims and knowledge approval.
- Research, authoring and reviewer AI jobs with explicit human gates.
- Products, editions, curricula, content units and versioned content.
- PublicationPackage generation and manual Kiwify publication confirmation.
- Kiwify webhook intake followed by authoritative API reconciliation.
- Learners, purchases, capstones, evaluations and outcome reporting.
- Audit events, operational dashboards, OpenAPI, AsyncAPI and observability.


### 2.3 Deferred or excluded


- Market Intelligence is a future module.
- Update Engine is P1.5, after the MVP loop is stable.
- Multi-tenant isolation, a public learner portal and mobile apps are outside MVP.
- Redis, LiteLLM Proxy, FastAPI and mandatory LangGraph/LangChain are excluded.
- Material UI is excluded. Refine is headless; shadcn/ui and Tailwind own presentation.


## 3. Architecture drivers and measurable quality attributes


### 3.1 Maintainability


- No forbidden project or module dependency may merge.
- Every hand-written C# file declares at most one type and its filename equals the type name.
- No generic repository, generic unit of work, BaseService, CrudService, Manager or Utils dumping ground.
- Domain/Application line coverage target: at least 90%; branch coverage target: at least 85%.
- Critical state transitions require explicit tests regardless of aggregate coverage.
- Shared business policy has one owner. Coincidentally similar syntax may remain separate until a stable abstraction exists.


### 3.2 Reliability


- Business messages use at-least-once delivery and idempotent consumers.
- Long HTTP commands return 202 plus an operation resource.
- State changes and outgoing messages commit atomically through the transactional outbox.
- Retry applies only to classified transient failures with bounded exponential backoff and jitter.
- Dead letters retain correlation, causation, schema version and failure classification.
- Backup target: RPO <= 24 hours and RTO <= 4 hours.


### 3.3 Security


- Unsafe browser requests require an authenticated SameSite cookie and anti-forgery token.
- Secrets never enter source control, messages, prompts, traces or logs.
- Ingestion defends against SSRF, oversized content, decompression bombs, unsafe HTML and unsupported media.
- AI tools are allowlisted, bounded and authorized by the .NET platform.


### 3.4 Performance baseline


- Read API p95 target is under 500 ms and ordinary command p95 under 800 ms, excluding asynchronous work.
- Dashboard and queues use dedicated projections; they do not load aggregates.
- Binary payloads never traverse RabbitMQ.
- Vector indexes are introduced from measured workloads; pgvector remains available from day one.


## 4. Runtime architecture


### 4.1 Containers


- Content Studio: React 19 + Vite, served by Caddy and same-origin with the API.
- Platform API: ASP.NET Core .NET 10, Wolverine.HTTP, Wolverine and EF Core.
- AI Worker: Python, Strands Agents, LiteLLM SDK, Pydantic, aio-pika, httpx and Tenacity.
- PostgreSQL: one database, schemas per module, pgvector and Wolverine durability.
- RabbitMQ: versioned cross-runtime commands and result events.
- Blob storage: filesystem implementation behind IBlobStore; object storage is replaceable.
- Caddy: TLS, secure headers, static assets and reverse proxy.


### 4.2 Ownership


The .NET platform owns business truth, identity, authorization, workflow and persistence. The AI worker owns temporary execution state only. RabbitMQ owns delivery, not business state. Blob storage owns bytes, not metadata.


### 4.3 Main flow


1. A human uses Content Studio over HTTPS.
2. The API authenticates and authorizes the request.
3. A domain transition and outgoing message commit in one transaction.
4. Wolverine publishes a CloudEvent to RabbitMQ.
5. The worker validates it, reads authorized blobs, invokes bounded tools/models and publishes a result.
6. A .NET consumer validates and applies the result idempotently.
7. The UI observes the operation resource and requires human approval at defined gates.


## 5. Technology baseline


- Backend: .NET 10, ASP.NET Core, Wolverine.HTTP, Wolverine, EF Core, Npgsql, PostgreSQL and pgvector.
- Backend support: Mapperly; Refit; Microsoft.Extensions.Http.Resilience.
- Frontend: React 19, TypeScript strict, Vite, Refine Core headless, shadcn/ui new-york, Tailwind CSS v4, Orval, TanStack Query/Table, React Hook Form, Zod, Lucide, Sonner and CVA.
- AI: Python, Strands Agents, LiteLLM SDK, Pydantic, aio-pika, httpx, Tenacity, Docling, Trafilatura, Playwright, Jinja2 and FFmpeg.
- Contracts: OpenAPI 3.1, JSON Schema, CloudEvents JSON and AsyncAPI.
- Observability: OpenTelemetry, Serilog, structlog and OTLP.
- Testing: xUnit, pytest, Testcontainers and Playwright.
- Deployment: Docker Compose, Caddy and a separate migration job.


Dependency versions are centrally pinned. Provider endpoints, model IDs, queues, timeouts, roles, feature thresholds and domain policy values MUST NOT be scattered through code.


## 6. Clean Architecture


### 6.1 Dependency rule


- ContentOS.Domain depends only on ContentOS.SharedKernel and the base class library.
- ContentOS.Application depends on Domain and SharedKernel; it owns use cases and ports.
- ContentOS.Infrastructure depends inward on Application, Domain and Contracts; it implements ports.
- ContentOS.Api is the composition root and contains no business policy.
- ContentOS.Contracts contains external HTTP/event schemas and no implementation dependencies.
- ContentOS.Migrations is an independently executable deployment component.
- Production code never references a test project.


### 6.2 Repository layout


```text
src/
  backend/
    ContentOS.Api/
    ContentOS.Application/
    ContentOS.Domain/
    ContentOS.Infrastructure/
    ContentOS.Contracts/
    ContentOS.SharedKernel/
    ContentOS.Migrations/
  studio/
  ai-worker/
contracts/
  http/openapi.json
  events/asyncapi.yaml
  events/schemas/
tests/
  ContentOS.Domain.UnitTests/
  ContentOS.Application.UnitTests/
  ContentOS.IntegrationTests/
  ContentOS.ArchitectureTests/
  ContentOS.ContractTests/
  ContentOS.EndToEndTests/
  ai-worker-tests/
ops/
  compose/
  caddy/
  observability/
docs/adr/
```


### 6.3 Layer responsibilities


#### Domain


Aggregates, entities, value objects, domain services, events and invariant-bearing policies. No EF attributes, HTTP concepts, broker concepts, provider SDKs, paths or configuration access.


#### Application


One use-case handler per command/query, validation, authorization calls, transaction metadata and narrow ports. Expected business failures use explicit results.


#### Infrastructure


PlatformDbContext, EF mappings, repositories, dedicated queries, Wolverine configuration, blob storage, Kiwify, outbound HTTP, clock/ID implementations and telemetry.


#### API


Endpoint definitions, request/response mapping, session/anti-forgery filters, ProblemDetails mapping, OpenAPI metadata and composition. Endpoints dispatch use cases; they do not implement workflow rules.


#### Contracts


Externally observable immutable DTOs and versioned schemas. Contract DTOs do not inherit domain entities.


### 6.4 Vertical slice organization


```text
ContentOS.Application/Knowledge/Sources/Create/
  CreateSourceCommand.cs
  CreateSourceHandler.cs
  CreateSourceValidator.cs
  CreateSourceResult.cs
ContentOS.Api/Knowledge/Sources/Create/
  CreateSourceEndpoint.cs
  CreateSourceRequest.cs
  CreateSourceResponse.cs
```


Feature folders preserve discoverability while layer projects preserve dependency direction. Do not create a project per module in the first release; module folders and architecture tests provide sufficient isolation.


## 7. Modular monolith boundaries


### 7.1 Modules


- Identity: users, capabilities, MFA, invitations and sessions.
- Knowledge: sources, snapshots, evidence, claims, embeddings and approval.
- Research: jobs, plans, attempts and findings.
- Content: units, versions, review decisions and generation lineage.
- Catalog: products, editions and curricula.
- Publication: packages, assets, exports and publication confirmation.
- Commerce: channels, webhook inbox, reconciliation, purchases and refunds.
- Learning: learners, enrollments, capstones, submissions, evaluations and outcomes.
- Market Intelligence: later; consumes public facts/events and cannot bypass current owners.


### 7.2 Module rules


- A module mutates only its aggregate roots and tables.
- Cross-module mutation uses an application command or integration event.
- Cross-module synchronous reads use explicit query ports with purpose-specific DTOs.
- Cross-schema foreign keys require an ADR and are limited to stable reference relationships.
- Public module contracts are intentionally small; entities are never shared.
- Cyclic dependencies fail architecture tests.


## 8. Domain model and invariants


### 8.1 Provenance graph


Source → SourceSnapshot → Evidence ↔ Claim ↔ ContentUnit → ContentVersion → Product → Edition → Distribution.


Named association entities represent meaningful many-to-many relationships and retain timestamps and provenance.


- SourceSnapshot is immutable and references a SHA-256 blob.
- Evidence identifies an exact snapshot location, extraction method and confidence.
- A Claim cannot be approved without supporting Evidence.
- Approved claims remain versioned; new snapshots create impact candidates rather than rewriting history.
- Editing approved content creates a successor ContentVersion.
- An Edition references explicit approved versions, never mutable “latest” during publication.
- PublicationPackage records source versions, renderer version and all asset hashes.
- A Purchase is authoritative only after commerce reconciliation.


### 8.2 Aggregate roots


- Source: canonical URI/upload identity, capture policy and lifecycle.
- Claim: statement, evidence links, confidence, review state and supersession.
- ResearchJob: requested scope, durable state and attempt lineage.
- ContentUnit: stable identity and version history.
- ContentVersion: immutable body, generation lineage, review and concurrency version.
- Product: commercial identity and editions.
- Edition: curriculum ordering, release and publication eligibility.
- PublicationPackage: build manifest, validation, export and confirmation.
- Purchase: provider references, money, status and reconciliation lineage.
- Capstone: requirements and rubric.
- Evaluation: rubric results, reviewer/agent lineage and outcome.


### 8.3 Value objects


SourceUri, ContentHash, EvidenceLocator, ConfidenceScore, Money, EmailAddress, CorrelationId, IdempotencyKey, SchemaVersion and ETagVersion validate on construction and are immutable. Use a value object when a primitive has constraints, behavior or confusing identity.


## 9. Workflow ownership and transitions


### 9.1 ResearchJob


Requested → Queued → Running → AwaitingKnowledgeReview → Completed.


Failure states: RetryScheduled, Failed and Cancelled.


Only Wolverine schedules durable retries. The worker reports an attempt result and never owns durable business state.


### 9.2 Claim


Draft → PendingReview → Approved or Rejected.


Approved → Superseded when replaced by a newer approved claim.


Rejection requires a reason. Approval records actor, time, evidence set and concurrency version.


### 9.3 ContentVersion


Draft → GenerationQueued → Generated → ReviewQueued → AgentReviewed → PendingHumanApproval → Approved or ChangesRequested.


Approved versions are immutable.


### 9.4 PublicationPackage


Requested → Building → Validating → ReadyForExport → Exported → PublishedConfirmed.


Export does not imply publication.


### 9.5 Purchase


SignalReceived → ReconciliationQueued → Confirmed, Refunded, Chargeback or Ignored.


A webhook is a signal. Provider API reconciliation is authoritative.


Aggregate methods or named domain policies own transitions. Endpoints, consumers and UI code MUST NOT assign status fields directly.


## 10. Persistence


### 10.1 Topology and access


Use one PostgreSQL database with schemas identity, knowledge, research, content, catalog, publication, commerce, learning and wolverine. One PlatformDbContext coordinates mappings and transactions.


To preserve Clean Architecture, Application handlers do not receive PlatformDbContext. They depend on aggregate-specific repositories and dedicated query ports implemented in Infrastructure.


- Repositories exist only for aggregate persistence. IRepository<T> is prohibited.
- Queries use purpose-specific ports such as IClaimReviewQueueQuery.
- IUnitOfWork is prohibited. EF Core plus Wolverine middleware owns commit.
- Raw SQL is permitted only for measured read hot paths/migrations in Infrastructure with integration tests.
- All timestamps are timestamptz UTC.
- Prefer UUIDv7 where supported by the pinned runtime/library.
- Use an explicit bigint version surfaced as a strong ETag.
- Soft delete is not a default; use lifecycle states or tombstones when the domain requires them.


### 10.2 Core relations


- knowledge.sources, source_snapshots, evidence, claims, claim_evidence, claim_impacts, embeddings.
- research.research_jobs, research_attempts, research_findings.
- content.content_units, content_versions, content_claims, review_decisions.
- catalog.products, editions, curriculum_items, edition_content_versions.
- publication.publication_packages, package_items, export_attempts, publication_confirmations.
- commerce.webhook_inbox, reconciliation_runs, purchases, purchase_events.
- learning.learners, enrollments, capstones, submissions, evaluations, outcomes.
- identity maps ASP.NET Core Identity into its schema.
- wolverine owns inbox, outbox, scheduled and saga durability tables.


### 10.3 Constraints and indexes


- Unique normalized canonical source URI where applicable.
- Unique content hash and storage key.
- Unique association pairs for claim_evidence and content_claims.
- Status + updated_at indexes for work queues.
- Provider + external_id uniqueness in Commerce.
- Check constraints for bounded confidence and legal persisted state values.
- Vector index type/parameters follow measured corpus and query behavior.
- Every migration has forward verification and rollback/roll-forward guidance.


### 10.4 Blob storage


IBlobStore exposes Put, OpenRead, Exists and metadata operations by content hash. The filesystem implementation writes to a temporary path and atomically renames into a deterministic SHA-256 path. Domain records retain hash, length, media type and purpose, never machine-specific paths.


## 11. HTTP contracts


### 11.1 Global rules


- Prefix: /api/v1.
- Single resources return the DTO directly.
- Collections return PageResponse<T>.
- Long work returns 202 OperationAccepted.
- Errors use application/problem+json.
- Versioned updates return ETag and require If-Match.
- Idempotent commands accept Idempotency-Key.
- Timestamps are ISO 8601 UTC; money is amount + ISO currency.
- camelCase JSON; unknown write enum values fail validation.
- Backend OpenAPI 3.1 is the only TypeScript client source.


### 11.2 Shapes


```json
PageResponse<T>
{
  "items": [T],
  "page": 1,
  "pageSize": 25,
  "totalItems": 240,
  "totalPages": 10
}
OperationAccepted
{
  "operationId": "uuid",
  "statusUrl": "/api/v1/operations/{id}",
  "correlationId": "uuid",
  "acceptedAt": "2026-09-17T12:00:00Z"
}
ProblemDetails
{
  "type": "https://contentos.dev/problems/concurrency-conflict",
  "title": "The resource changed",
  "status": 412,
  "detail": "Reload and review the latest version.",
  "instance": "/api/v1/content-versions/{id}",
  "code": "CONTENT_VERSION_STALE",
  "traceId": "...",
  "correlationId": "...",
  "retryable": false,
  "currentVersion": 8,
  "errors": { "title": ["Required"] }
}
```


### 11.3 Endpoint inventory


#### Dashboard and Sources


- GET /api/v1/dashboard/summary
- GET, POST /api/v1/sources
- GET /api/v1/sources/{sourceId}
- GET, POST /api/v1/sources/{sourceId}/snapshots
- GET /api/v1/sources/{sourceId}/impact


#### Research and Knowledge


- POST /api/v1/research-jobs
- GET /api/v1/research-jobs/{jobId}
- GET /api/v1/research-jobs/{jobId}/claims
- GET /api/v1/claims/{claimId}
- POST /api/v1/claims/{claimId}/approve
- POST /api/v1/claims/{claimId}/reject
- GET /api/v1/claims/{claimId}/impact


#### Content and Catalog


- GET /api/v1/content-units
- GET, PUT /api/v1/content-versions/{versionId}
- POST /api/v1/content-versions/{versionId}/request-review
- POST /api/v1/content-versions/{versionId}/approve
- GET /api/v1/products/{productId}/editions/{editionId}
- PUT /api/v1/editions/{editionId}/curriculum


#### Publication, Commerce and Learning


- POST /api/v1/editions/{editionId}/publication-packages
- GET /api/v1/publication-packages/{packageId}
- POST /api/v1/publication-packages/{packageId}/export
- POST /api/v1/publication-packages/{packageId}/confirm-publication
- GET /api/v1/purchases
- POST /api/v1/commerce/reconciliation-runs
- GET /api/v1/outcomes/summary


#### Identity and Administration


- POST /api/v1/auth/login
- POST /api/v1/auth/mfa/verify
- GET /api/v1/auth/session
- POST /api/v1/auth/logout
- GET /api/v1/users
- POST /api/v1/user-invitations
- PUT /api/v1/users/{userId}/role
- GET, PUT /api/v1/integrations/{integrationKey}
- GET /api/v1/audit-events


### 11.4 Error-to-UI semantics


- 400: malformed/cross-field validation; visible summary.
- 401: redirect to login and preserve a safe return location.
- 403: dedicated forbidden state.
- 404: not found or concealed.
- 409: business conflict or invalid transition.
- 412: stale edit; compare/reload and never silently overwrite.
- 422: bind errors to form fields.
- 429: honor Retry-After; do not replay commands automatically.
- 5xx: retry automatically only when retryable=true and safe; show correlationId.
- Offline: preserve an unsent draft and reconcile against the latest ETag before saving.


## 12. Messaging contracts


### 12.1 Envelope


Cross-runtime messages use CloudEvents JSON with JSON Schema payloads and AsyncAPI documentation. Required metadata: messageId/id, source, type, specversion, occurredAt/time, subject, correlationId, causationId, idempotencyKey and schemaVersion.


```json
{
  "specversion": "1.0",
  "id": "uuid",
  "source": "contentos.platform",
  "type": "com.contentos.research.requested.v1",
  "subject": "research-job/{jobId}",
  "time": "2026-09-17T12:00:00Z",
  "datacontenttype": "application/json",
  "correlationid": "uuid",
  "causationid": "uuid",
  "idempotencykey": "research:{jobId}:attempt:{n}",
  "schemaversion": 1,
  "data": {}
}
```


### 12.2 Topology


- Topic exchange: contentos.ai.
- Commands: ai.research, ai.content.generate, ai.content.review, ai.documents.normalize.
- Results: backend.ai.results with versioned routing keys.
- Dead-letter exchange/queue per boundary and restricted replay tooling.
- Concurrency/prefetch are workload-specific.
- Messages contain blob references and hashes, never binaries or unbounded prompts.


### 12.3 Delivery semantics


- Wolverine transactional inbox/outbox is mandatory for .NET.
- Python maintains a durable processing ledger or deterministic result IDs.
- Consumers acknowledge only after durable success or terminal handling.
- Poison messages dead-letter after bounded attempts and alert operations.
- Replay preserves the original message ID and creates a replay-attempt ID.
- Schema changes are additive within v1; breaking changes publish a coexisting version.


## 13. AI worker


### 13.1 Responsibilities


- Consume validated jobs, execute bounded agent workflows, produce artifacts and publish typed results.
- Never own product workflow, authorization or database truth.
- Never receive PostgreSQL credentials.
- Access knowledge only through restricted internal API tools with workload identity.
- Treat retrieved text and tool output as untrusted data.
- Record logical model, resolved provider/model, prompt version, tools, tokens, latency and hashes.


### 13.2 Packages


```text
ai_worker/
  bootstrap/
  contracts/
  consumers/
  agents/
  tools/
  model_gateway/
  extraction/
  rendering/
  telemetry/
  settings/
  tests/
```


Pydantic validates all ingress/egress. Strands coordinates model/tool interaction inside a job. LiteLLM SDK is the provider-neutral model boundary; no proxy in MVP. Tenacity retries only transient failures. aio-pika handles RabbitMQ and httpx calls internal APIs.


Docling is the primary converter. Trafilatura handles suitable web pages and Playwright is a controlled rendering fallback. Jinja2 renders text templates and FFmpeg performs approved media transformations. Every tool has size limits, timeouts and telemetry.


### 13.3 Model configuration


- Business code uses aliases such as research.standard and review.strict.
- Alias mapping, temperature, context, timeout, retry and fallback are typed configuration.
- Fallback cannot weaken capability or residency policy silently.
- Model output is parsed and validated; it is never applied as a direct domain command.
- Prompt templates are versioned tested assets.
- Cost/token budgets are enforced by job type.


## 14. Frontend architecture and contract alignment


### 14.1 Design system


The Figma file is the visual/interaction baseline. Page 08 defines frontend/backend contracts and page 09 locks the frontend stack. shadcn/ui new-york supplies accessible source-owned components; Tailwind CSS v4 expresses tokens/utilities. Refine remains headless.


- CSS custom properties are the token source.
- CVA owns finite variants; repeated arbitrary classes are extracted only for proven reuse.
- Lucide is the icon source. Sonner handles transient notifications; actionable errors remain on page.
- Controls have keyboard, focus, loading, empty, error, success and disabled states.
- WCAG 2.2 AA is the acceptance target.


### 14.2 Data flow


- Orval generates the only platform HTTP client and TanStack Query hooks.
- Feature code MUST NOT call fetch/Axios directly for platform APIs.
- Zod provides early feedback; the server is authoritative.
- React Hook Form maps 422 field paths.
- ETags are retained and sent through If-Match.
- One Idempotency-Key is generated per user intent and retained across safe network retries.
- Contract fixtures verify serialization, errors and enum compatibility.
- UTC is formatted to locale only at the presentation edge.


### 14.3 Screen mapping


- Dashboard uses /dashboard/summary and never recomputes business totals.
- Source list/detail/impact use source/snapshot APIs; snapshots appear immutable.
- Research monitor uses POST/GET research-jobs and displays operation state.
- Claim review approves/rejects with If-Match and explicit reason.
- Content editor uses content-version APIs and preserves drafts offline.
- Edition builder sends an explicit ordered curriculum.
- Publication keeps export and publication confirmation separate.
- Commerce never labels a webhook receipt as a confirmed sale.
- Outcomes consume a backend summary DTO; formulas are not duplicated in React.
- Admin navigation is role-aware while the API remains authoritative.


## 15. C# engineering standards


### 15.1 One type per .cs file — mandatory


Every hand-written .cs file declares at most one class, record, record struct, struct, enum, interface or delegate. The filename exactly matches the type. This includes tests.


- Allowed: CreateSourceCommand.cs contains only CreateSourceCommand.
- Prohibited: SourceModels.cs contains SourceDto, SourceStatus and CreateSourceRequest.
- Prohibited: nested request/response/result types in an endpoint or handler.
- Prohibited: Enums.cs, Dtos.cs, Models.cs, Contracts.cs and Helpers.cs bundles.
- A partial type is allowed only for generated integration or a documented framework boundary; each file still declares only that same type.


Narrow exceptions: tool/compiler-generated files, EF migration designer output, AssemblyInfo.cs, GlobalUsings.cs and top-level Program.cs when it declares no type. Generated output is never hand-edited.


A Roslyn analyzer and architecture test MUST count declarations per syntax tree, ignore generated files by deterministic marker/path, verify filename equality and fail CI.


### 15.2 Clean Code


- Enable nullable references, file-scoped namespaces and warnings-as-errors.
- Use immutable request/result records.
- Use domain vocabulary; avoid unexplained abbreviations.
- Handlers coordinate one use case; domain behavior stays with aggregates/value objects/policies.
- Use guard clauses and explicit results; expected business failures are not exceptions.
- No boolean flags selecting behavior; use separate commands or strategies.
- No magic strings/numbers; use typed options, value objects and policy objects.
- Use TimeProvider and an injected ID generator.
- No static mutable state, service locator or ambient context.
- Keep methods cohesive.
- Comments explain why or external constraints, not obvious code.


### 15.3 SOLID


- SRP: one handler per use case, one adapter per external concern, invariants in aggregates.
- OCP: add extractors, blob stores or channels behind stable ports.
- LSP: all implementations pass the same behavioral contract suite.
- ISP: narrow interfaces such as ISourceSnapshotReader instead of a broad storage service.
- DIP: Domain/Application own abstractions; Infrastructure implements; Api composes.


### 15.4 Reuse and duplication


- Extract when a shared business concept has one owner, three stable repetitions expose the same change axis, or a boundary/security rule must be uniform immediately.
- Do not unify code because current syntax happens to look alike.
- SharedKernel stays tiny: IDs, domain-event abstraction and universal stable primitives only.
- Typed Options own configuration reuse and validate at startup.
- Mapperly maps each boundary; domain entities are never response DTOs.
- A problem-code catalog owns error semantics.
- Named authorization policies replace distributed role-string comparisons.


## 16. GoF design patterns


### 16.1 Approved problem-pattern mappings


- Strategy: source extraction, publication rendering, commerce reconciliation and model selection.
- Adapter: Kiwify, filesystem/object storage, provider SDKs and AI internal-tool clients.
- Factory Method + Resolver: choose a registered strategy by SourceKind/channel in one composition-owned place.
- Builder: create a deterministic PublicationPackage manifest with validated required parts.
- Observer: domain/integration events notify decoupled modules through Wolverine.
- Decorator: resilience, telemetry and authorization around ports, preferably standard pipelines.
- Chain of Responsibility: validation, authorization, idempotency, transaction and telemetry middleware.
- Facade: restricted internal Knowledge Tool API for AI access.
- State: use separate state objects only after transition behavior becomes genuinely polymorphic; start with aggregate methods and a transition table.
- Composite: only if nested content blocks require uniform traversal/rendering.


### 16.2 Guardrails


- Every pattern proposal names the variation, alternatives and removed duplication.
- A single-implementation interface requires a boundary, test seam or expected provider variation.
- Do not recreate mediator, bus, retry or pipeline frameworks already supplied by Wolverine/.NET.
- Specification is not GoF; use it sparingly for reusable domain query criteria.
- Remove a pattern when its indirection exceeds the variation it isolates.


## 17. Representative C# slice


The comments indicate separate files; the types MUST NOT be placed together.


```csharp
// CreateSourceCommand.cs
public sealed record CreateSourceCommand(
    Uri Location,
    SourceKind Kind,
    string DisplayName,
    IdempotencyKey IdempotencyKey);
// ISourceRepository.cs
public interface ISourceRepository
{
    Task<bool> CanonicalLocationExistsAsync(
        SourceUri location,
        CancellationToken cancellationToken);
    void Add(Source source);
}
// CreateSourceHandler.cs
public sealed class CreateSourceHandler
{
    private readonly ISourceRepository _sources;
    private readonly TimeProvider _clock;
    private readonly IIdGenerator _ids;
    public async Task<CreateSourceResult> Handle(
        CreateSourceCommand command,
        CancellationToken cancellationToken)
    {
        var location = SourceUri.Create(command.Location);
        if (await _sources.CanonicalLocationExistsAsync(
            location, cancellationToken))
        {
            return CreateSourceResult.Duplicate(location);
        }
        var source = Source.Create(
            _ids.NewSourceId(),
            location,
            command.Kind,
            command.DisplayName,
            _clock.GetUtcNow());
        _sources.Add(source);
        return CreateSourceResult.Created(source.Id);
    }
}
```


Commit, idempotency storage and outbox dispatch are pipeline concerns, never copied into each handler.


## 18. Security


### 18.1 Identity and access


- ASP.NET Core Identity with Secure, HttpOnly, SameSite cookies and HTTPS outside development.
- MFA required for Admin and recommended for Reviewer.
- Anti-forgery required for unsafe browser methods.
- Capabilities such as knowledge.approve and publication.confirm are named policies; roles are bundles.
- Login/MFA/recovery use rate limits, lockout and audit.
- Revoke sessions after password reset, role reduction or suspicious activity.


### 18.2 Ingestion and application


- Permit only approved URL schemes; safely resolve DNS.
- Deny loopback, link-local, private networks and cloud metadata addresses.
- Revalidate every redirect and final destination.
- Enforce media type, extension, byte, page, duration and decompression limits.
- Sanitize rendered HTML and set a restrictive Content Security Policy.
- Store integration secrets encrypted and redact them from logs.
- Verify webhook signatures where available; always reconcile.
- Block critical dependency, container and secret-scan findings.


### 18.3 AI safety


- Treat document instructions as evidence content, never trusted control instructions.
- No general shell, database or arbitrary network tool.
- Allowlist outbound destinations per tool/environment.
- Require Pydantic and business validation before accepting output.
- Keep human approval for claims, final content and publication confirmation.


## 19. Observability and audit


- OpenTelemetry spans cover HTTP, Wolverine, PostgreSQL, RabbitMQ, Python, Strands, tool calls and model calls.
- Propagate W3C trace context through HTTP and CloudEvents.
- Structured logs require service, environment, traceId, spanId, correlationId, operationId, messageId and errorCode as applicable.
- Metrics cover latency/error, queue depth/age, outbox age, AI duration/token/cost, extraction failure and time-in-state.
- Audit events are immutable business/security records; logs are not the audit ledger.
- Do not log source bodies, full prompts, tokens, cookies, secrets or personal data by default.
- Alerts focus on symptoms: error ratio, old work, stuck workflow, failed reconciliation and failed backup.


## 20. Testing


### 20.1 Layers


- Domain unit tests: invariants, value objects, transitions and policies.
- Application unit tests: orchestration with deterministic ports, TimeProvider and IDs.
- Architecture tests: layer/module dependencies, cycles, naming, forbidden abstractions and one type per file.
- Integration tests: Testcontainers PostgreSQL/RabbitMQ with real EF and Wolverine.
- Contract tests: OpenAPI examples, ProblemDetails, ETags, idempotency and event schemas.
- Adapter contract suites: the same behavior against each implementation.
- Python tests: Pydantic contracts, tools, failure classification and duplicate delivery.
- Playwright: pt-BR loading, empty, success, 400/401/403/404/409/412/422/429/5xx and offline states.
- End-to-end: real Postgres/RabbitMQ and fake external providers.


### 20.2 Critical MVP scenario


1. Create Product and Edition.
2. Register Source and capture immutable Snapshot.
3. Run Research and produce evidence-linked Claims.
4. Human Reviewer approves Knowledge.
5. Author Agent generates a ContentVersion.
6. Reviewer Agent evaluates it and requests human approval.
7. Human approves and builds a deterministic PublicationPackage.
8. Export, publish manually in Kiwify and confirm.
9. Receive webhook, reconcile with Kiwify API and create Purchase/Learner.
10. Submit/evaluate Capstone and update Outcome.


Every step preserves lineage/correlation. Duplicate delivery never duplicates business effects.


## 21. CI/CD gates


1. Restore/install locked dependencies and validate license/security policy.
2. Format, lint and compile with warnings as errors.
3. Run architecture tests before slower suites.
4. Run unit tests and scoped coverage gates.
5. Run PostgreSQL/RabbitMQ integration tests.
6. Generate OpenAPI 3.1, AsyncAPI and JSON Schemas.
7. Lint and breaking-diff contracts against the released baseline.
8. Run Orval and fail if git diff is dirty.
9. Compile strict TypeScript and run frontend contract tests.
10. Run Python lint, type checks, tests and fixtures.
11. Run Playwright state and critical-path tests.
12. Build signed/scanned images and an SBOM.
13. Run migration job, deploy services, smoke-test and promote.


All gates are branch-protected. Production services MUST NOT call Database.Migrate() at startup.


## 22. Deployment and recovery


- Docker Compose deploys Caddy, Studio/API, worker, PostgreSQL, RabbitMQ and the OTLP collector/exporters.
- Run as non-root with read-only filesystems where practical and least-privilege networks.
- Readiness checks dependencies required to accept work; liveness avoids restart storms.
- Graceful shutdown stops intake and preserves safe redelivery.
- Migrations run once with advisory locking.
- Back up PostgreSQL, blobs and encrypted configuration to another failure domain.
- Run quarterly restore drills and verify database/blob consistency.
- RabbitMQ is not a backup.
- Feature flags are typed, centrally registered and have owner/removal dates.


## 23. Hardcode prevention


- Use validated RabbitMqOptions, BlobStoreOptions, AiModelOptions, CommerceOptions, SecurityOptions and RetentionOptions.
- Bind configuration only at the composition root; Domain never reads IConfiguration.
- Define queues/exchanges once in messaging topology.
- Centralize problem codes, permissions, limits, retry classifications and media types.
- Version templates/prompts as assets rather than scattered multiline literals.
- Store provider identifiers as external references, not domain IDs.
- Business constants belong to named tested policies.
- Defaults are deliberate, documented and startup-validated. Unsafe silent fallback is prohibited.


## 24. Delivery plan


### Phase 0 — Foundation


Repository, projects, analyzers, architecture tests, Compose, identity skeleton, observability, contracts and design tokens.


### Phase 1 — Knowledge loop


Sources, snapshots, blobs, extraction, evidence, claims, review UI and provenance.


### Phase 2 — AI content loop


Research, author/reviewer agents, model gateway, content versions, gates and operation monitoring.


### Phase 3 — Product and publication


Products, editions, curriculum, package builder, deterministic export and confirmation.


### Phase 4 — Commerce and outcomes


Kiwify, webhook inbox, reconciliation, purchases, learners, capstones, evaluations and outcomes.


### Phase 5 — Hardening/P1.5


Recovery drills, performance, security review, Update Engine and additional channels/providers.


### MVP definition of done


- Full critical scenario passes in CI and production-like infrastructure.
- No unresolved critical/high security finding.
- No unapproved breaking contract change.
- Architecture rules pass, including one type per .cs.
- Critical workflows remain idempotent under duplicate delivery.
- Human approval/provenance are visible in UI and audit.
- A restore drill meets RPO/RTO.
- Dashboards and alerts exist before launch.


## 25. Architecture Decision Records


- ADR-001: modular monolith before microservices.
- ADR-002: one PostgreSQL database and one PlatformDbContext.
- ADR-003: application ports instead of DbContext in handlers.
- ADR-004: Wolverine owns durable orchestration; Strands owns bounded AI execution.
- ADR-005: RabbitMQ transports messages, not files.
- ADR-006: no Redis until a measured need exists.
- ADR-007: LiteLLM SDK in-process, no proxy in MVP.
- ADR-008: filesystem behind IBlobStore.
- ADR-009: shadcn/ui + Tailwind v4 + Refine headless; no Material UI.
- ADR-010: one hand-written C# type per file, automatically enforced.
- ADR-011: no generic repository/unit of work/base service.
- ADR-012: GoF patterns require a named variation and testable payoff.
- ADR-013: Kiwify webhook is a signal; reconciliation is authoritative.
- ADR-014: migrations are a deployment job, never API startup.
- ADR-015: observability is OTLP backend-agnostic.


## 26. Pull request review checklist


- Do dependencies point inward?
- Is behavior owned by the correct module/aggregate?
- Is every hand-written C# type in its own correctly named file?
- Did the change avoid generic repositories, service locators and base-service inheritance?
- Is reused logic truly one concept with one owner?
- Are configuration/policy named, typed and free of scattered literals?
- Does each interface isolate a real boundary or variation?
- Does a GoF pattern solve a documented problem?
- Are HTTP/event contracts versioned and tested?
- Are retries safe, bounded and idempotent?
- Are authorization, provenance, audit and telemetry present?
- Does the UI implement loading, empty, error, conflict and offline states?
- Can the change be replaced without rewriting unrelated modules?


## 27. Official references


- ASP.NET Core OpenAPI: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview?view=aspnetcore-10.0
- ASP.NET Core ProblemDetails: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0
- Wolverine RabbitMQ: https://wolverinefx.net/guide/messaging/transports/rabbitmq/
- Wolverine EF inbox/outbox: https://wolverinefx.net/guide/durability/efcore/outbox-and-inbox
- Wolverine sagas: https://wolverinefx.net/guide/durability/sagas
- shadcn/ui: https://ui.shadcn.com/docs
- Tailwind CSS: https://tailwindcss.com/docs
- Orval: https://orval.dev/
- Refine: https://github.com/refinedev/refine
- Strands Agents: https://strandsagents.com/
- LiteLLM SDK: https://docs.litellm.ai/
- Docling: https://docling-project.github.io/
- CloudEvents: https://github.com/cloudevents/spec
- AsyncAPI: https://www.asyncapi.com/docs


## 28. Glossary


- Aggregate: consistency boundary that protects invariants.
- Application port: narrow interface owned inward and implemented externally.
- Claim: reviewable statement supported by evidence.
- Evidence: addressable observation tied to an immutable snapshot.
- Operation: durable asynchronous work representation exposed to UI.
- PublicationPackage: deterministic manifest and assets prepared for distribution.
- Snapshot: immutable capture of a source.
- Outbox/inbox: durable mechanism coupling state changes and reliable messaging.
- Idempotency: repeated handling has the same business effect as one handling.
- Provenance: lineage from source bytes through evidence, claims, content and publication.


End of Software Design Document.