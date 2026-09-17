# Architecture review

- [ ] Dependencies point inward (Clean Architecture)
- [ ] Behavior owned by correct module/aggregate
- [ ] One hand-written C# type per correctly named file
- [ ] No generic repo / custom UoW / base-service / utils dump
- [ ] Reuse has one owner; no wrong-module shared domain
- [ ] Config/policy typed; no scattered literals
- [ ] Interfaces isolate a real boundary or variation
- [ ] GoF (if any) names the variation and payoff
- [ ] HTTP/event contracts versioned; ProblemDetails/ETag/idempotency respected
- [ ] Retries bounded, classified, idempotent
- [ ] AuthZ, provenance, audit, telemetry present where required
- [ ] UI: loading/empty/error/conflict/offline; Figma-aligned for Studio
- [ ] Python has no domain DB access
- [ ] Migrations not run from API startup
