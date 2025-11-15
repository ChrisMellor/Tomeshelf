# Tomeshelf
Tomeshelf is a small .NET 9 that pulls Comic Con guest data from an external API, stores it in SQL Server via EF Core, and exposes the data through an HTTP API and a simple MVC web UI. It uses .NET Aspire to orchestrate the app (API + Web + SQL) in development, with OpenTelemetry and health checks wired in.

## Status
[![Build (main)](https://github.com/ChrisMellor/Tomeshelf/actions/workflows/dotnet-ci.yml/badge.svg?branch=main)](https://github.com/ChrisMellor/Tomeshelf/actions/workflows/dotnet-ci.yml?query=branch%3Amain)
[![Tests (main)](https://img.shields.io/github/actions/workflow/status/ChrisMellor/Tomeshelf/dotnet-ci.yml?branch=main&label=tests)](https://github.com/ChrisMellor/Tomeshelf/actions/workflows/dotnet-ci.yml?query=branch%3Amain)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)

[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=bugs)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)

[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=coverage)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)


**Highlights**

- Persists events, people, categories, schedules in SQL Server (EF Core 9)
- API for querying and force-refreshing guests, with Swagger UI in Development
- MVC site for browsing guests by city (`/comiccon/city/{city}/guests`)
- Background job refreshes guests hourly for configured cities
- .NET Aspire for service discovery, SQL container, health, and telemetry
- Executor scheduler runs as an Aspire-managed service with UI + Quartz jobs to trigger downstream APIs

## Project Layout

- `src/Tomeshelf.Api` — ASP.NET Core API, controllers, Swagger, migrations on startup
- `src/Tomeshelf.Web` — ASP.NET Core MVC site consuming the API
- `src/Tomeshelf.Infrastructure` — EF Core DbContext, queries, HTTP client, ingest services
- `src/Tomeshelf.Application` - DTOs and options shared across layers
- `src/Tomeshelf.Domain` - Entity classes and relationships
- `src/Tomeshelf.ServiceDefaults` - OpenTelemetry, health checks, service discovery helpers
- `src/Tomeshelf.AppHost` - .NET Aspire AppHost defining API, Web, SQL resources
- `Tomeshelf.Executor` - Quartz-based scheduler that triggers API refresh endpoints over HTTP
- `tests/*` - Unit tests for API, Web, Infrastructure, Application

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

## Executor Scheduler

- `Tomeshelf.Executor` hosts a Quartz scheduler that calls the HTTP endpoints responsible for refreshing downstream data (Comic Con guests, Humble Bundle listings, etc.).
- The scheduler binds the `Executor` configuration section and registers one Quartz job + trigger per endpoint entry.
- Each job issues an HTTP request using the configured method, URL, headers, and cron expression, and logs the outcome. The host references `Tomeshelf.ServiceDefaults`, so the outbound HTTP client benefits from the same service discovery, resilience, telemetry, and health conventions as the rest of the distributed app.
- A lightweight UI is available at `/executor` to list, add, edit, and delete endpoints; it persists changes to `executorSettings*.json`, reloads configuration automatically, and syncs the running scheduler without restarts.

Example configuration:

```jsonc
{
  "Executor": {
    "Enabled": true,
    "Endpoints": [
      {
        "Name": "comiccon-london-refresh",
        "Url": "https://localhost:7000/api/ComicCon/Guests/City?city=London",
        "Method": "POST",
        "Cron": "0 0 * * * ?",
        "TimeZone": "UTC",
        "Headers": {
          "X-Api-Key": "optional-token"
        }
      }
    ]
  }
}
```

- `Cron` uses Quartz expressions (`{seconds} {minutes} {hours} {day-of-month} {month} {day-of-week}`) and defaults to UTC when no `TimeZone` is supplied.
- Override any value per environment via standard configuration providers (e.g., `Executor__Endpoints__0__Url` in env vars).
- Disable the entire scheduler by setting `Executor:Enabled` to `false`, or disable a specific job via `Endpoints[n].Enabled`.

## Development

- Build: `dotnet build Tomeshelf.sln`
- Test: `dotnet test`
- CI on main: see latest runs and artifacts
  - https://github.com/ChrisMellor/Tomeshelf/actions/workflows/dotnet-ci.yml?query=branch%3Amain
  - The CI uploads `coverage-results` (Coverlet) for each run
- Static analysis: provide `SONAR_TOKEN`, `SONAR_ORG`, and `SONAR_PROJECT_KEY` secrets to enable the SonarCloud scan in CI
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
