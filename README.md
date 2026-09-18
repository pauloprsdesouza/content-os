# Content OS

Internal content operations platform: sources → claims → learning content → publication → commerce → outcomes.

## Docs

- Full SDD: `Content-OS-SDD-v1.0.md`
- Agent lean pack: `.cursor/` / `AGENTS.md`

## Stack

- Platform API: .NET 10, Wolverine, EF Core, PostgreSQL
- Content Studio: React 19, Vite, Refine headless, shadcn/ui, Tailwind v4
- AI Worker: Python, Strands Agents, LiteLLM SDK, RabbitMQ
- Ops: Docker Compose, Caddy

## Quick start (local → Homelab, no Docker for apps)

Secrets: Doppler (`ops/doppler/README.md`). Apps run on the laptop; Postgres/RabbitMQ on Homelab (`192.168.10.13`).

```powershell
doppler login
doppler setup   # uses doppler.yaml → epilogik-platform / local_contentos

.\ops\doppler\run-migrations.ps1
.\ops\doppler\run-api.ps1
# optional: .\ops\doppler\run-ai-worker.ps1

cd src/studio
npm install
npm run dev   # http://localhost:5173 → proxies /api to :5231
```

Compose under `ops/compose` remains available for an all-local infra stack if Homelab is unreachable.
