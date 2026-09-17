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

## Quick start (local)

```bash
cp ops/compose/.env.example ops/compose/.env
docker compose -f ops/compose/docker-compose.yml up -d
```

See `ops/compose/README.md` after Phase 0 lands.
