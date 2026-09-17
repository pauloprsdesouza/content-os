# System SDD — Content OS

Status: Active · Kind: product MVP · Last reviewed: 2026-09-17  
UI locale: pt-BR · Figma: https://www.figma.com/design/ws9h9RE7NGrE9hVbwqRnxY

## Outcome

Internal content ops platform: trusted sources → researched claims → approved learning content → product editions → publication packages → sales → learner outcomes. Full lineage, human gates, replaceable integrations.

## Boundary

**In:** Identity (cookie + MFA), Knowledge, Research, Content, Catalog, Publication, Commerce (Kiwify), Learning, audit, OpenAPI/AsyncAPI, OTEL, Docker Compose deploy.

**Out:** Market Intelligence, Update Engine (P1.5), multi-tenant isolation, public learner portal, mobile, Redis, LiteLLM Proxy, FastAPI, Material UI, mandatory LangGraph/LangChain, CI/CD gates (deferred; direct deploy for MVP).

## Deployables

| Deployable | Role | SoR |
|---|---|---|
| Content Studio | React 19 admin (Refine headless + shadcn/Tailwind) | none |
| Platform API | .NET 10 modular monolith, Wolverine, Identity | PostgreSQL |
| AI Worker | Python Strands + LiteLLM SDK | none (temp exec only) |
| PostgreSQL | one DB, schemas per module + wolverine + pgvector | — |
| RabbitMQ | CloudEvents commands/results | delivery only |
| Blob store | filesystem behind `IBlobStore` | bytes only |
| Caddy | TLS, static Studio, reverse proxy API | — |

## Modules (one DB, folder isolation)

Identity · Knowledge · Research · Content · Catalog · Publication · Commerce · Learning

Cross-module writes via application command or integration event only. No shared entities across modules.

## Main flow

Human → Studio (HTTPS) → API authz → domain + outbox in one TX → CloudEvent → worker → result → .NET idempotent apply → UI polls Operation → human approval at gates.

## MVP critical path

Product/Edition → Source/Snapshot → Research/Claims → approve Knowledge → generate ContentVersion → agent review → human approve → PublicationPackage → export → manual Kiwify publish + confirm → webhook → reconcile → Purchase/Learner → Capstone/Outcome.

## Constraints

- .NET owns business truth; Python has **no** DB credentials.
- Binary payloads never on RabbitMQ (blob refs + hashes only).
- Migrations are a separate job — API MUST NOT `Database.Migrate()` at startup.
- Direct deploy to server for MVP (no CI/CD required yet).
