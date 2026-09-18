# Environments

| Env | Purpose | Deploy / secrets |
|---|---|---|
| Local | Laptop apps → Homelab infra (`192.168.10.13`) | `doppler run -c local_contentos` (see `ops/doppler/README.md`); optional `ops/compose` if Homelab down |
| Development | Apps on Homelab host | Doppler `dev_contentos` |
| Hostinger | MVP production (VPS Debian) | Doppler `prd_contentos`; direct deploy (no CI/CD yet) |

## MVP deploy policy

- Prefer local processes + Homelab infra for day-to-day; Compose under `ops/compose` is fallback infra only.
- Run `ContentOS.Migrations` once with advisory locking before promoting API.
- Same artifact digest preferred Homelab → Hostinger when Homelab is used for certification.
- Secrets via Doppler — never committed.
- Do not point Local PC at Hostinger production DB by default.
