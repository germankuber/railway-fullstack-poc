# Railway Fullstack POC

Todo app: .NET 10 Web API (Identity bearer auth, EF Core, PostgreSQL, SignalR) and a Vite + React + TypeScript web client.

## Run locally

    docker compose up -d --build

Web: http://localhost:5173, API: http://localhost:8080 (`/health`).
Stop and wipe data: `docker compose down -v`.

## Deploy to Railway

1. Create a project with a PostgreSQL service.
2. Add service `api` from this repo, root directory `apps/api` (Dockerfile build).
   Variables: `DATABASE_URL=${{Postgres.DATABASE_URL}}`, `WEB_ORIGIN=https://<web-domain>`.
3. Add service `web`, root directory `apps/web` (Dockerfile build).
   Build variable: `VITE_API_URL=https://<api-domain>`.
4. Generate public domains for `api` and `web`, then set the two URLs above and redeploy.

`PORT` is injected by Railway for both services.

## Database migrations

Migrations run once per deploy, before the new version starts, never at application startup.

- Railway: `deploy.preDeployCommand = ["dotnet TodoApi.dll --migrate"]` in `.railway/railway.ts`. If it fails, the deploy is aborted and the previous version keeps serving traffic.
- Local: the `migrate` service in `docker-compose.yml` runs the same command and `api` waits for it to complete successfully.
- New migration: `cd apps/api && dotnet tool restore && dotnet ef migrations add <Name> -o Data/Migrations`.

## Infrastructure as code

Railway service settings live in `.railway/railway.ts` (variables stay unmanaged in Railway).

```bash
pnpm install
railway config plan
railway config apply
```

## Logs

The API writes one JSON line per event (`level`, `message` and structured attributes), so Railway can filter them:

```bash
railway logs -s api -f "@level:error"
railway logs -s api -f "@userId:<id>"
railway logs -s api --http --status ">=400" -n 50
```
