# Environments

| Env | Purpose | Deploy |
|---|---|---|
| Local | Dev against Compose (Postgres, RabbitMQ, Caddy) | `ops/compose` |
| Homelab | Shared infra / validation when available | Compose on LAN host |
| Hostinger | MVP production target (VPS Debian) | Direct deploy (no CI/CD yet) |

## MVP deploy policy

- Build images locally (or on the server); `docker compose` up.
- Run `ContentOS.Migrations` once with advisory locking before promoting API.
- Same artifact digest preferred Homelab → Hostinger when Homelab is used for certification.
- Secrets via env / host secret store — never committed.
- Do not point Local PC at Hostinger production DB by default.
