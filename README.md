# Tomeshelf

Tomeshelf is a small .NET 9 that pulls Comic Con guest data from an external API, stores it in SQL Server via EF Core, and exposes the data through an HTTP API and a simple MVC web UI. It uses .NET Aspire to orchestrate the app (API + Web + SQL) in development, with OpenTelemetry and health checks wired in.

**Highlights**

- Persists events, people, categories, schedules in SQL Server (EF Core 9)
- API for querying and force-refreshing guests, with Swagger UI in Development
- MVC site for browsing guests by city (`/comiccon/city/{city}/guests`)
- Background job refreshes guests hourly for configured cities
- .NET Aspire for service discovery, SQL container, health, and telemetry

## Project Layout

- `src/Tomeshelf.Api` — ASP.NET Core API, controllers, Swagger, migrations on startup
- `src/Tomeshelf.Web` — ASP.NET Core MVC site consuming the API
- `src/Tomeshelf.Infrastructure` — EF Core DbContext, queries, HTTP client, ingest services
- `src/Tomeshelf.Application` — DTOs and options shared across layers
- `src/Tomeshelf.Domain` — Entity classes and relationships
- `src/Tomeshelf.ServiceDefaults` — OpenTelemetry, health checks, service discovery helpers
- `src/Tomeshelf.AppHost` — .NET Aspire AppHost defining API, Web, SQL resources
- `tests/*` — Unit tests for API, Web, Infrastructure, Application

## Prerequisites

- `.NET 9 SDK` (the repo targets `net9.0`)
- `Docker` (for local SQL Server via Aspire AppHost)
- Optional: `Azure SQL` if running against cloud instead of local SQL container

## Quick Start (Recommended: .NET Aspire)

1) Configure Comic Con sites via AppHost user secrets (one or more entries):

   - `dotnet user-secrets init --project src/Tomeshelf.AppHost`
   - `dotnet user-secrets set "ComicCon:0:City" "London" --project src/Tomeshelf.AppHost`
   - `dotnet user-secrets set "ComicCon:0:Key" "{GUID}" --project src/Tomeshelf.AppHost`
   - `dotnet user-secrets set "ComicCon:1:City" "Birmingham" --project src/Tomeshelf.AppHost`
   - `dotnet user-secrets set "ComicCon:1:Key" "{GUID}" --project src/Tomeshelf.AppHost`

2) Run the distributed app (API + Web + SQL):

   - `dotnet run -p src/Tomeshelf.AppHost`

   The AppHost starts a SQL Server container and wires service discovery so the Web app can reach the API at `http://api`. The API applies EF Core migrations on startup (`TomeshelfDbContext.Database.Migrate()`).

3) Browse the apps:

   - Web UI: the AppHost output lists the `web` service URL (external HTTP endpoint)
   - API Swagger UI (Development): the `api` service root shows Swagger (OpenAPI) docs
   - Health endpoints (Development): `/health` and `/alive` on both services

## Running Without Aspire

You can run the API and Web projects directly. Configure connection strings and endpoints yourself.

1) API configuration

- Add an appsettings or user-secrets for the API project with:

  - `ConnectionStrings:tomeshelfdb` — SQL Server connection string
  - `ComicCon` — array of `{ City, Key }` entries

Example user-secrets for API:

```
dotnet user-secrets init --project src/Tomeshelf.Api
dotnet user-secrets set "ConnectionStrings:tomeshelfdb" "Server=localhost;Database=tomeshelfdb;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=true;" --project src/Tomeshelf.Api
dotnet user-secrets set "ComicCon:0:City" "London" --project src/Tomeshelf.Api
dotnet user-secrets set "ComicCon:0:Key" "{GUID}" --project src/Tomeshelf.Api
```

Run the API:

- `dotnet run -p src/Tomeshelf.Api`

2) Web configuration

- When not using Aspire’s service discovery, set the API base URL:

  - `Services:ApiBase` — e.g., `http://localhost:5026`

Example user-secrets for Web:

```
dotnet user-secrets init --project src/Tomeshelf.Web
dotnet user-secrets set "Services:ApiBase" "http://localhost:5026" --project src/Tomeshelf.Web
```

Run the Web site:

- `dotnet run -p src/Tomeshelf.Web`

## Data Model

- `Event` — external id, name, slug; has many `EventAppearance`
- `Person` — biographical and social fields; images, categories, appearances
- `EventAppearance` — join of `Event` + `Person` with pricing, booth and days; has many `Schedule`
- `Category` — many-to-many via `PersonCategory`
- `Schedule` — per-appearance sessions with optional `VenueLocation`

EF Core constraints:

- Unique: `Event.ExternalId`, `Event.Slug`, `Category.ExternalId`, `Person.ExternalId`, `Schedule (EventAppearanceId, ExternalId)`
- FK: `EventAppearance.EventId -> Events.Id`, `EventAppearance.PersonId -> People.Id`, etc.

## API Endpoints

- `POST /api/ComicCon/Guests/City?city=London`
  - Triggers an on-demand refresh from the external API and returns the latest people.

- `GET /api/ComicCon/Guests/City?city=london`
  - Returns an object: `{ city, total, groups }` where each group is a date bucket with `items` of people DTOs, including schedules and categories.

In Development, visit the API root for Swagger UI.

## Web Routes

- `GET /comiccon/city/{city}/guests` — Browses grouped guests for a city (e.g., `London`, `Birmingham`).

## Background Updates

- `ComicConUpdateBackgroundService` runs hourly in the API host
- Iterates the configured cities and invokes the ingest flow

## Development

- Build: `dotnet build Tomeshelf.sln`
- Test: `dotnet test`
- Apply migrations (if creating new ones):

  - Add migration: `dotnet ef migrations add Name -s src/Tomeshelf.Api -p src/Tomeshelf.Infrastructure`
  - Update DB: `dotnet ef database update -s src/Tomeshelf.Api -p src/Tomeshelf.Infrastructure`

The API applies pending migrations automatically on startup.

## Observability

- OpenTelemetry tracing and metrics are configured; set `OTEL_EXPORTER_OTLP_ENDPOINT` to enable an OTLP exporter
- Health checks in Development: `/health` and `/alive`

## Troubleshooting

- No guests returned: verify correct Comic Con `Key` GUID for the city
- SQL connection errors: confirm `ConnectionStrings:tomeshelfdb` and SQL reachability
- Web cannot reach API when running standalone: set `Services:ApiBase` to the API URL
- EF foreign key conflicts: ensure relationships are created via tracked navigation properties (handled by the ingest service)

