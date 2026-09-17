# Feature — MVP critical path

Status: Active · Phase: 0–4 · Last updated: 2026-09-17

## Outcome

Ship a usable Content OS loop: source → claim approval → content approval → publish → sale → learner outcome, with provenance and human gates.

## Acceptance (DoD)

1. Create Product and Edition.
2. Register Source; capture immutable Snapshot.
3. Run Research; produce evidence-linked Claims.
4. Human Reviewer approves Knowledge.
5. Author agent generates ContentVersion.
6. Reviewer agent evaluates; requests human approval.
7. Human approves; build deterministic PublicationPackage.
8. Export; publish manually in Kiwify; confirm in app.
9. Webhook → reconcile via Kiwify API → Purchase/Learner.
10. Capstone submit/evaluate → Outcome updated.

Every step keeps lineage/correlation. Duplicate delivery never duplicates effects. Architecture rules (incl. one type/file) pass. Direct deploy works on target server without CI/CD.

## Out

Market Intelligence, Update Engine, multi-tenant, public portal, Redis, LiteLLM proxy, Material UI.
