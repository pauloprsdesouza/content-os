# Content OS — Doppler secrets

Secrets live in Doppler. Never commit values. Prefer `doppler run` over copying into `.env`.

## Workplace constraint (current)

The Epilogik Doppler workplace is at the **10-project limit**, so a dedicated `content-os` project could not be created yet.

Until a slot is free (upgrade plan or archive an unused project), Content OS uses **branch configs** on `epilogik-platform`:

| Intent | Doppler project | Config | Meaning |
|---|---|---|---|
| Local laptop → Homelab infra | `epilogik-platform` | `local_contentos` | Apps on localhost; Postgres/RabbitMQ at `192.168.10.13` |
| Development on Homelab host | `epilogik-platform` | `dev_contentos` | Apps on `192.168.10.13`; infra via loopback |
| Production (Hostinger VPS) | `epilogik-platform` | `prd_contentos` | `srv1793047.hstgr.cloud` / `187.127.39.214` — fill `REPLACE_ME_*` before deploy |

When a project slot exists, run `.\ops\doppler\setup-project.ps1` to create `content-os` with envs `local` / `dev` / `prd` and migrate keys.

Epilogik naming reference: `local` = laptop, `dev` = Homelab, `prd` = Hostinger production (see `epilogik-infra` doppler docs).

## Redis

**N/A for MVP** (`REDIS_NOTE` in Doppler). ADR-0006 excludes Redis; this repo has no Redis dependency.

## One-time setup

```powershell
doppler login
# optional: pin this repo to the Content OS config
doppler setup --project epilogik-platform --config local_contentos
```

## Run (PowerShell)

From repo root:

```powershell
# Migrations (Homelab Postgres)
doppler run -p epilogik-platform -c local_contentos -- `
  dotnet run --project src/backend/ContentOS.Migrations

# API (http://localhost:5231)
doppler run -p epilogik-platform -c local_contentos -- `
  dotnet run --project src/backend/ContentOS.Api --launch-profile http

# AI worker (stub)
doppler run -p epilogik-platform -c local_contentos -- powershell -NoProfile -Command {
  Set-Location src/ai-worker
  if (-not (Test-Path .venv)) { python -m venv .venv; .\.venv\Scripts\python -m pip install -e . }
  .\.venv\Scripts\python -m ai_worker.bootstrap.app
}

# Studio (no secrets required; proxies /api → :5231)
Set-Location src/studio
npm install
npm run dev
```

Or use the helpers:

```powershell
.\ops\doppler\run-migrations.ps1
.\ops\doppler\run-api.ps1
.\ops\doppler\run-ai-worker.ps1
```

## Key names (no values)

Registered on `local_contentos` / `dev_contentos` / `prd_contentos` (plus inherited `epilogik-platform` infra keys on branch configs):

| Area | Names |
|---|---|
| Hosting | `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS`, `DOTNET_ENVIRONMENT` |
| Postgres | `CONNECTIONSTRINGS__PLATFORM` |
| Blobs | `BLOBSTORE__ROOTPATH` |
| RabbitMQ | `RABBITMQ__URI`, `RABBITMQ__AIREQUESTSEXCHANGE`, `RABBITMQ__AIRESULTSQUEUE` |
| Worker auth | `AIWORKER__INTERNALAPIKEY` |
| Commerce | `COMMERCE__USESTUBPROVIDER`, `COMMERCE__BASEURL`, `COMMERCE__WEBHOOKSECRET`, `COMMERCE__APIKEY`, `COMMERCE__DEFAULTPRODUCTID`, `COMMERCE__DEFAULTEDITIONID` |
| Dev seed | `DEVELOPMENTSEED__ENABLED`, `DEVELOPMENTSEED__ADMINEMAIL`, `DEVELOPMENTSEED__ADMINPASSWORD`, `DEVELOPMENTSEED__SEEDDEMOKNOWLEDGE`, `DEVELOPMENTSEED__SEEDDEMOCATALOG` |
| Security | `SECURITY__COOKIENAME`, `SECURITY__SESSIONLIFETIME` |
| AI worker | `CONTENT_OS_AI_STUB`, `CONTENT_OS_RABBITMQ_URL`, `CONTENT_OS_API_BASE_URL`, `CONTENT_OS_WORKER_API_KEY`, `CONTENT_OS_ENVIRONMENT` |
| URLs | `CONTENTOS_API_PUBLIC_URL`, `CONTENTOS_STUDIO_ORIGIN`, `CONTENTOS_HOMELAB_HOST` (local/dev), `CONTENTOS_HOSTINGER_HOST` / `CONTENTOS_HOSTINGER_IPV4` (prd) |
| OTEL | `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_EXPORTER_OTLP_PROTOCOL`, `OTEL_RESOURCE_ATTRIBUTES` |
| Redis | `REDIS_NOTE` (= N/A) |

Doppler secret names are **UPPERCASE**; ASP.NET Core env binding is case-insensitive (`CONNECTIONSTRINGS__PLATFORM` → `ConnectionStrings:Platform`).

## Production placeholders

`prd_contentos` has public Hostinger identity filled. Replace before any Hostinger deploy:

- `CONNECTIONSTRINGS__PLATFORM` (`REPLACE_ME_HOSTINGER_POSTGRES`)
- `RABBITMQ__URI` / `CONTENT_OS_RABBITMQ_URL` (`REPLACE_ME_HOSTINGER_RABBITMQ`)

Do not point the laptop `local_contentos` config at Hostinger production databases.

## Homelab notes

- Homelab LAN host: `192.168.10.13` (Postgres `:5432`, RabbitMQ `:5672`).
- Database `contentos` (pgvector when available) is expected on Homelab Postgres.
- OTLP `:4317` may be closed from the LAN; apps still start if export fails closed or is unused.
