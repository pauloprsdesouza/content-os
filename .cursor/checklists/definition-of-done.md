# Definition of done

- [ ] Acceptance criteria of the feature SDD met
- [ ] Architecture rules pass (deps + one type per `.cs` file)
- [ ] Relevant unit/integration/contract tests added or updated
- [ ] OpenAPI/AsyncAPI updated if contracts changed; Studio Orval regenerated if needed
- [ ] No secrets in source/logs; Options used for config
- [ ] Human gates / provenance visible where required
- [ ] Idempotent under duplicate delivery (if messaging touched)
- [ ] Migrated via job (not API startup); rollback/roll-forward noted
- [ ] Smoke on Compose / target server after direct deploy
- [ ] Feature SDD / ADR updated if decisions changed
