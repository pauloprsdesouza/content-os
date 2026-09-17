# Engineering standards

Status: Active · Last reviewed: 2026-09-17

Executable detail lives in `.cursor/rules/`. This file states non-negotiable intent.

## Mandatory

1. Clean Architecture physically enforced — Domain/Application never depend on Infrastructure.
2. One hand-written C# type per `.cs` file; filename = type name.
3. No generic repository, custom UoW, BaseService, CrudService, Manager, Utils dumping ground.
4. No hardcoding of secrets, URLs, queues, model IDs, timeouts, roles, or env hosts — typed Options at composition root.
5. Handlers = one use case; aggregates own transitions; endpoints do not set status fields.
6. UTC internally; locale format only at UI edge.
7. Timeouts on every external call; retries only for classified transient + idempotent ops.
8. Tests at lowest useful level; architecture tests fail forbidden deps and one-type-per-file.

## Stack map

| Area | Rule | Anchors |
|---|---|---|
| All | `00-core-engineering.mdc` | Clean Code, pragmatic SOLID |
| Design qualities | `11-design-qualities.mdc` | componentization, DRY-with-judgment |
| Architecture | `10-architecture-boundaries.mdc` | Clean Architecture, pragmatic GoF |
| Vertical slices | `30-cqrs-vertical-slices.mdc` | feature folders |
| Persistence | `33-persistence.mdc` | EF + Wolverine commit; ports not DbContext in handlers |
| .NET | `20-dotnet.mdc` | .NET 10, Wolverine, EF, Mapperly, ProblemDetails |
| Python | `21-python.mdc` | Strands, Pydantic, aio-pika, LiteLLM SDK |
| Studio | `22-typescript-studio.mdc` | React 19, Refine headless, shadcn, Orval, Figma SoT |

## Layout (repo)

```text
src/backend/{Api,Application,Domain,Infrastructure,Contracts,SharedKernel,Migrations}/
src/studio/  src/ai-worker/
contracts/{http,events}/  tests/  ops/{compose,caddy,observability}/  docs/adr/
```

## Explicitly out of style

- Service locator / ambient mutable context
- Speculative frameworks for one use case
- Sharing domain entities as HTTP DTOs
- Logging secrets, cookies, full prompts, source bodies
- Embedding provider details in Domain/Application
