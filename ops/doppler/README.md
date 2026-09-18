# Content OS — Doppler secrets

Secrets live in Doppler project **`content-os`**. Never commit values. Prefer `doppler run`.

| Intent | Config | Meaning |
|---|---|---|
| Local laptop → Homelab infra | `local` | Apps on localhost; Postgres/RabbitMQ at `192.168.10.13` |
| Development on Homelab host | `dev` | Apps on `192.168.10.13`; infra via loopback |
| Production (Hostinger VPS) | `prd` | `srv1793047.hstgr.cloud` / `187.127.39.214` — fill `REPLACE_ME_*` before deploy |

Also available: `stg`, `dev_personal` (unused by default scripts).

## Redis

**N/A for MVP** (`REDIS_NOTE`). ADR-0006 excludes Redis.

## One-time setup

```powershell
doppler login
doppler setup --project content-os --config local
```

## Run (PowerShell)

```powershell
.\ops\doppler\run-migrations.ps1
.\ops\doppler\run-api.ps1
.\ops\doppler\run-ai-worker.ps1
# Studio
cd src/studio; npm run dev
```

Or:

```powershell
doppler run -p content-os -c local -- dotnet run --project src/backend/ContentOS.Api --launch-profile http
```

## Key names (no values)

| Area | Names |
|---|---|
| Hosting | `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS`, `DOTNET_ENVIRONMENT` |
| Postgres | `CONNECTIONSTRINGS__PLATFORM` |
| Blobs | `BLOBSTORE__ROOTPATH` |
| RabbitMQ | `RABBITMQ__URI`, `RABBITMQ__AIREQUESTSEXCHANGE`, `RABBITMQ__AIRESULTSQUEUE` |
| Worker auth | `AIWORKER__INTERNALAPIKEY` |
| Commerce | `COMMERCE__USESTUBPROVIDER`, `COMMERCE__BASEURL`, `COMMERCE__WEBHOOKSECRET`, `COMMERCE__APIKEY`, … |
| Dev seed | `DEVELOPMENTSEED__*` |
| Security | `SECURITY__COOKIENAME`, `SECURITY__SESSIONLIFETIME` |
| AI worker | `CONTENT_OS_AI_STUB`, `CONTENT_OS_RABBITMQ_URL`, `CONTENT_OS_API_BASE_URL`, `CONTENT_OS_WORKER_API_KEY` |
| URLs | `CONTENTOS_API_PUBLIC_URL`, `CONTENTOS_STUDIO_ORIGIN`, `CONTENTOS_HOMELAB_HOST` / Hostinger hosts |
| OTEL | `OTEL_*` |

## Production placeholders

Before Hostinger deploy, replace in `prd`:

- `CONNECTIONSTRINGS__PLATFORM` (`REPLACE_ME_HOSTINGER_POSTGRES`)
- `RABBITMQ__URI` / `CONTENT_OS_RABBITMQ_URL` (`REPLACE_ME_HOSTINGER_RABBITMQ`)

Do not point `local` at Hostinger production databases.

## Homelab

- Host: `192.168.10.13` (Postgres `:5432`, RabbitMQ `:5672`)
- Database: `contentos`
