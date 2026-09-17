# Architecture Decision Records

Create an ADR when a decision affects multiple deployables, changes a boundary/contract, is costly to reverse, or supersedes an accepted decision. Do not ADR ordinary implementation already covered by rules.

| ADR | Title | Status |
|---|---|---|
| [0001](0001-modular-monolith.md) | Modular monolith before microservices | Accepted |
| [0002](0002-one-postgres-one-dbcontext.md) | One PostgreSQL DB + one PlatformDbContext | Accepted |
| [0003](0003-application-ports.md) | Application ports instead of DbContext in handlers | Accepted |
| [0004](0004-wolverine-strands.md) | Wolverine orchestration; Strands AI execution | Accepted |
| [0005](0005-rabbitmq-messages-not-files.md) | RabbitMQ transports messages, not files | Accepted |
| [0006](0006-no-redis-until-measured.md) | No Redis until measured need | Accepted |
| [0007](0007-litellm-sdk-in-process.md) | LiteLLM SDK in-process; no proxy in MVP | Accepted |
| [0008](0008-filesystem-blobstore.md) | Filesystem behind IBlobStore | Accepted |
| [0009](0009-studio-stack.md) | shadcn + Tailwind v4 + Refine headless | Accepted |
| [0010](0010-one-type-per-file.md) | One hand-written C# type per file | Accepted |
| [0011](0011-no-generic-repo-uow.md) | No generic repository / UoW / base service | Accepted |
| [0012](0012-gof-requires-named-variation.md) | GoF patterns need named variation | Accepted |
| [0013](0013-kiwify-webhook-vs-reconcile.md) | Kiwify webhook signal; reconcile authoritative | Accepted |
| [0014](0014-migrations-as-job.md) | Migrations are a deployment job | Accepted |
| [0015](0015-otel-backend-agnostic.md) | Observability OTLP backend-agnostic | Accepted |
| [0016](0016-mvp-direct-deploy.md) | MVP direct deploy; CI/CD deferred | Accepted |
