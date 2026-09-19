# Feature — Phase 2 AI content loop

Status: Active · Phase: 2 · Last updated: 2026-09-18

## Outcome

Research jobs, author/reviewer agents, model gateway aliases, ContentUnit/ContentVersion lifecycle, Operation monitoring, human gates. The Studio path is one “Novo conteúdo” wizard: required format, then either the user’s materials or on-demand topics.

## Local MVP (shipped)

- Domain: `ResearchJob` (+ findings), `ContentUnit`/`ContentVersion` with `ContentFormat`, minimal `Operation`.
- Formats are named strategies (`Newsletter`, `Post`, `Aula`, `Ebook`, `Artigo`). The generate command carries the format mold. The model does not receive a URL to browse.
- “Meus materiais” reuses source capture (URL, file, pasted text). Citation is by content hash and the existing excerpt API.
- On-demand topics: user picks an OpenAlex field or subfield and a 7/30/90 day window. `IScholarlyLiterature` is the Application port; `OpenAlexLiteratureSearch` is the Infrastructure adapter. Options: `OpenAlex:BaseUrl`, `MailTo`, `ApiKey`, `TimeoutSeconds`, `MaxWorks`. Secrets stay in configuration and are not logged.
- The adapter reconstructs inverted-index abstracts. An empty search stays empty. The label command only includes works already retrieved. A topic with no known work id is rejected.
- Selected topics become `text/plain` snapshots through the existing capture/blob/excerpt path. The draft cites those hashes.
- Editorial series (format, area, window, cadence manual/weekly/monthly, owner) live in Content. Wolverine schedules the next collection (ADR-0004). A cadence run only creates topic candidates. It does not generate or publish.
- Each content unit stores an optional `ProductId`. The wizard requires a product before format. Topic selection and series confirmation pass the same id. Approved versions still enter the product only through the edition curriculum.
- Studio navigation follows the use-case IA: Agora, Edições, Revisão, Fontes, Desempenho. Account stays in the avatar. Legacy paths redirect.
- Studio can create a product (and its first edition) and discard a product, source, research job, content unit, or editorial series. Discard is refused when the record is already in use: sales or enrollments, cited evidence, claims under review, or a curriculum placement.
- API: research-jobs, content-units/versions (If-Match gates), topic-areas, topic-discoveries, editorial-series, operations, internal snapshot-by-hash tool API. DELETE on products, sources, research-jobs, content-units, and editorial-series. Product list includes the earliest edition id.
- Messaging: RabbitMQ CloudEvents (`contentos.ai` / `backend.ai.results`); worker stub when `CONTENT_OS_AI_STUB=true` or no LiteLLM key. New command: `ai.topics.label`.
- Studio follows operation status with SSE (`GET /api/v1/operations/{id}/events`), not client polling.
- Studio labels the main path in task language (Novo conteúdo, Formato, Meus materiais, Afirmações para revisar; coletando temas, escolha os temas, escrevendo, pronto para revisar) without removing the existing pages.

## Invariants

- Worker reports attempt results; never owns durable workflow state.
- Only Wolverine schedules durable retries and series cadence (MVP: RabbitMQ consumer applies results idempotently).
- ContentVersion: Draft → … → PendingHumanApproval → Approved|ChangesRequested; approved immutable.
- Model output validated; never applied as raw domain command.
- Topic labels must cite retrieved work ids. The model must not invent topics when the search is empty.

## Messaging

Commands: `ai.research`, `ai.content.generate`, `ai.content.review`, `ai.topics.label`, `ai.documents.normalize`. Results: `backend.ai.results`.

## Test plan

Duplicate delivery; schema validation; Operation SSE until a terminal status; budget/timeout enforcement. OpenAlex adapter against a recorded response, not the network. Reject a topic with no work id.
