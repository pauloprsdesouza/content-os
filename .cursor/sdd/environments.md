# Environments

| Env | Doppler (`content-os`) | Purpose |
|---|---|---|
| Local | `local` | Laptop apps → Homelab Postgres/RabbitMQ (`192.168.10.13`) via `doppler run` |
| Development | `dev` | Apps on Homelab host; infra loopback |
| Production | `prd` | Hostinger VPS — fill `REPLACE_ME_*` before deploy |

See `ops/doppler/README.md`.

## MVP deploy policy

- Prefer local processes + Homelab infra; Compose under `ops/compose` is fallback only.
- Run `ContentOS.Migrations` once with advisory locking before promoting API.
- Secrets in Doppler only — never committed.
- Do not point `local` at Hostinger production DB.
