# Takwene Track Management

A .NET 10 API and Angular frontend for browsing music tracks, artists, and DSP distributions.

## Requirements

- .NET 10 SDK
- Docker with Docker Compose, running
- Node.js >=24.15.0 <25 and npm 11.x
- Available ports: 5432 (PostgreSQL), 5000 (API), and 4200 (frontend)

## 1. Start the database

From the repository root:

```bash
cd backend
docker compose up -d
docker compose exec postgres pg_isready -U takwene -d takwene_db
```

Wait until PostgreSQL reports `accepting connections` before continuing.
The local database is `takwene_db`, with username and password `takwene`.

## 2. Apply migrations and run the API

In the same terminal, from `backend/`:

```bash
dotnet restore
dotnet tool restore
dotnet ef database update --project src/TakweneTask.Infrastructure --startup-project src/TakweneTask.Api
dotnet run --project src/TakweneTask.Api --launch-profile http
```

The migration creates the schema and seeds 4 artists, 3 DSPs, and 10 tracks with sample distributions.
Run the same `dotnet ef database update` command whenever there are new migrations; the API does not apply them automatically.

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

## 3. Run the frontend

In a second terminal, from the repository root:

```bash
cd frontend
npm ci
npm start
```

Open http://localhost:4200/tracks. The frontend proxies `/api` to port 5000.
Browsing tracks requires no login. If you use nvm, run `nvm install` and `nvm use` inside `frontend/` before installing dependencies.

## 4. Obtain and use a JWT token

With the API running, log in using the demo credentials. These examples use Bash and `curl`:

```bash
curl --request POST http://localhost:5000/api/auth/login \
  --header 'Content-Type: application/json' \
  --data '{"username":"demo","password":"demo-secret-key-12345"}'
```

Copy `accessToken` from the response. It expires after 60 minutes; log in again for a new token.
Use it as `Authorization: Bearer <token>` on write endpoints. For example:

```bash
TOKEN='paste-accessToken-here'

curl --request POST http://localhost:5000/api/artists \
  --header "Authorization: Bearer $TOKEN" \
  --header 'Content-Type: application/json' \
  --data '{"name":"Demo Artist","email":"demo.artist@example.com","country":"EG"}'
```

Artist and track GET endpoints are public. Creating records, distributing tracks, and updating statuses require a token.
The committed credentials and signing key are demo values; replace them through external configuration before deployment.

AutoMapper 15 accepts a license key through `AUTOMAPPER_LICENSE_KEY`; without one it emits a licensing warning. See the [publisher’s licensing documentation](https://docs.automapper.io/en/stable/License-configuration.html).

## Checks and shutdown

From `backend/`, run `dotnet test`. From `frontend/`, run `npm run build` and `npm run typecheck`.

To stop, press `Ctrl+C` in both application terminals, then run `docker compose down` from `backend/`.
The database volume preserves your data.

See [DECISIONS.md](DECISIONS.md) for the AI disclosure and [backend/README.md](../backend/README.md) for API details.
