# Deploy on Ubuntu (systemd + reverse proxy)

This repo is a .NET 9 solution with:
- `Api`: ASP.NET Core Web API
- `WorkerService`: background worker (optional)
- `Ui.Blazor`: Blazor WASM (static files)

The recommended production setup on Ubuntu is:
- Run `Api` and `WorkerService` as `systemd` services
- Put a reverse proxy in front (nginx) or use Cloudflare Tunnel
- Inject secrets via environment variables (never commit secrets)

## 1) Publish

From repo root:

```bash
dotnet publish Api/Api.csproj -c Release -o /var/www/citizenship/api
dotnet publish WorkerService/WorkerService.csproj -c Release -o /var/www/citizenship/worker
```

## SQL Server (Docker on the same Ubuntu host)

This repo includes a sample compose file: [docker-compose.sqlserver.yml](../docker-compose.sqlserver.yml).

1) Create a local `.env` (do not commit) based on [.env.example](../.env.example)
2) Start SQL Server:

```bash
docker compose -f docker-compose.sqlserver.yml --env-file .env up -d
docker ps
```

Notes:
- The volume `citizenship_sql_data` persists your database across restarts.
- If your API runs on the same host, prefer using `Server=127.0.0.1,1433` in the connection string.

## 2) Environment variables (secrets)

Create an env file for the API:

`/etc/citizenship/api.env`

```env
ASPNETCORE_ENVIRONMENT=Production

# Bind only to localhost when using a reverse proxy
ASPNETCORE_URLS=http://127.0.0.1:8080

# SQL Server
ConnectionStrings__DefaultConnection=Server=<host>,1433;Database=CitizenshipApp;User Id=<user>;Password=<password>;TrustServerCertificate=True

# JWT
Jwt__Issuer=CitizenshipApp
Jwt__Audience=CitizenshipApp.Ui
Jwt__Key=<64+ random chars>

# CORS for production (example)
Cors__AllowedOrigins__0=https://your-ui-domain.com

# If you are behind nginx/cloudflared and want the app to respect X-Forwarded-* headers
Proxy__Enabled=true
Proxy__ForwardLimit=2

# Optional
HttpsRedirection__HttpsPort=443
```

## 3) systemd unit files

## Database migrations (production)

This solution stores EF Core migrations in `Infrastructure` (see `Infrastructure/Persistence/Migrations`).

Recommended safe flow:
1) **Backup the database**.
2) Run migrations during a maintenance window (some schema changes may lock tables).

### Option A (recommended): run EF migrations from the repo (requires .NET SDK)

This repo uses a local tool manifest at `.config/dotnet-tools.json` (so you don't need to install `dotnet-ef` globally).

```bash
cd /home/dev/projects/CitizenshipApp

# Load production env vars (ConnectionStrings__DefaultConnection, Jwt__*, etc.)
set -a
source /etc/citizenship/api.env
set +a

# Restore local dotnet tools (dotnet-ef)
dotnet tool restore

# Preview migrations
dotnet tool run dotnet-ef migrations list \
    --project Infrastructure/Infrastructure.csproj \
    --startup-project Api/Api.csproj

# Apply migrations
dotnet tool run dotnet-ef database update \
    --project Infrastructure/Infrastructure.csproj \
    --startup-project Api/Api.csproj \
    --no-build
```

Rollback to a previous migration (example):

```bash
dotnet tool run dotnet-ef database update <PreviousMigrationNameOrId> \
    --project Infrastructure/Infrastructure.csproj \
    --startup-project Api/Api.csproj
```

### Option B: generate an idempotent SQL script (DBA-friendly)

```bash
cd /home/dev/projects/CitizenshipApp
dotnet tool restore

dotnet tool run dotnet-ef migrations script \
    --idempotent \
    --project Infrastructure/Infrastructure.csproj \
    --startup-project Api/Api.csproj \
    -o /tmp/citizenship-migrate.sql
```

Then apply `/tmp/citizenship-migrate.sql` using your preferred SQL Server tooling.

### API service

`/etc/systemd/system/citizenship-api.service`

```ini
[Unit]
Description=CitizenshipApp API
After=network.target

[Service]
Type=simple
WorkingDirectory=/var/www/citizenship/api
ExecStart=/usr/bin/dotnet /var/www/citizenship/api/Api.dll
Restart=always
RestartSec=5

EnvironmentFile=/etc/citizenship/api.env

# Hardening (optional)
NoNewPrivileges=true
PrivateTmp=true

[Install]
WantedBy=multi-user.target
```

### Worker service (optional)

`/etc/citizenship/worker.env`

```env
DOTNET_ENVIRONMENT=Production
```

`/etc/systemd/system/citizenship-worker.service`

```ini
[Unit]
Description=CitizenshipApp Worker
After=network.target

[Service]
Type=simple
WorkingDirectory=/var/www/citizenship/worker
ExecStart=/usr/bin/dotnet /var/www/citizenship/worker/WorkerService.dll
Restart=always
RestartSec=5

EnvironmentFile=/etc/citizenship/worker.env

NoNewPrivileges=true
PrivateTmp=true

[Install]
WantedBy=multi-user.target
```

Enable + start:

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now citizenship-api
sudo systemctl enable --now citizenship-worker

sudo systemctl status citizenship-api --no-pager
journalctl -u citizenship-api -f
```

## 4) Reverse proxy options

### Option A: nginx (recommended)

- Proxy to `http://127.0.0.1:8080`
- Terminate TLS at nginx (or at Cloudflare)

Example server block (sketch):

```nginx
server {
    listen 443 ssl;
    server_name api.your-domain.com;

    location / {
        proxy_pass         http://127.0.0.1:8080;
        proxy_http_version 1.1;

        proxy_set_header Host              $host;
        proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### Option B: Cloudflare Tunnel (cloudflared)

If you run cloudflared on the same host, it will forward requests to the local API.
When using cloudflared/nginx, set `Proxy__Enabled=true` so the API respects `X-Forwarded-Proto`.

## 5) Health checks

API exposes:
- `GET /health/live` (process up)
- `GET /health/ready` (includes DB check)

Use `/health/ready` for load balancer / tunnel readiness.
