# Content OS local infrastructure

Phase 0 runs only shared infrastructure in Compose. Run the API, Studio, and AI
worker directly from their source folders while those images are still evolving.

## Start

```powershell
Copy-Item ops/compose/.env.example ops/compose/.env
docker compose --env-file ops/compose/.env -f ops/compose/docker-compose.yml up -d
```

Before sharing or deploying this configuration, replace both placeholder
passwords in `.env`.

## Application processes

```powershell
# API (when its Phase 0 project is available)
dotnet run --project src/platform-api/ContentOs.Api

# Studio: http://localhost:5173
Set-Location src/studio
npm install
npm run dev

# Worker
Set-Location src/ai-worker
python -m venv .venv
.\.venv\Scripts\python -m pip install -e .
.\.venv\Scripts\python -m ai_worker.bootstrap.app
```

The Studio development server proxies `/api` to `http://localhost:5080`.

## Ports

- PostgreSQL: `5432`
- RabbitMQ AMQP: `5672`
- RabbitMQ management: `15672`
- Platform API (local process): `5080`
- Vite Studio: `5173`
- Caddy optional edge: `8080`

## Optional Caddy edge

Build the Studio first, then enable the `edge` profile:

```powershell
Set-Location src/studio
npm run build
Set-Location ../..
docker compose --env-file ops/compose/.env -f ops/compose/docker-compose.yml --profile edge up -d
```

Caddy serves `src/studio/dist` and proxies `/api` to the host API on port 5080.
The named `blobs` volume is reserved for the future filesystem blob-store
adapter; no Phase 0 service mounts it yet.
