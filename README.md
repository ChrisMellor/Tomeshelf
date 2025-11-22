# Tomeshelf
Tomeshelf is a data-aggregation platform built on .NET 10 and orchestrated with Aspire. It collects information from Comic Con, Humble Bundle, Fitbit, and FFXIV housing, processes it through a suite of targeted ASP.NET Core APIs, and surfaces everything through a unified MVC web front end. A scheduler ingests external feeds, builds curated bundles, and uploads them to Google Drive via OAuth. The system uses EF Core with SQL Server for persistence, includes full observability through health checks and OpenTelemetry, and is designed for clean, predictable local orchestration with Aspire.

## Status
### Pipeline
[![Build (main)](https://github.com/ChrisMellor/Tomeshelf/actions/workflows/dotnet-ci.yml/badge.svg?branch=main)](https://github.com/ChrisMellor/Tomeshelf/actions/workflows/dotnet-ci.yml?query=branch%3Amain)
[![Tests (main)](https://img.shields.io/github/actions/workflow/status/ChrisMellor/Tomeshelf/dotnet-ci.yml?branch=main&label=tests)](https://github.com/ChrisMellor/Tomeshelf/actions/workflows/dotnet-ci.yml?query=branch%3Amain)
### Code Quality (SonarCloud)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
### Code Health
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=coverage)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=bugs)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=ChrisMellor_Tomeshelf&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=ChrisMellor_Tomeshelf)

## Tech Stack
- **.NET 10** – Core runtime and framework  
- **Aspire** – Orchestration, config, and service wiring  
- **ASP.NET Core** – APIs + MVC front end  
- **Quartz.NET** – Scheduler for ingest and refresh jobs  
- **EF Core + SQL Server** – Storage and querying  
- **Google Drive API (OAuth)** – Bundle uploads  
- **OpenTelemetry** – Tracing, instrumentation, health checks  
- **Docker / Aspire Compose** – Local container environment  



## Features
### Aspire Orchestration
- **Aspire AppHost** – Coordinates all services, manages service discovery, configuration, and local orchestration.
### Web Layer
- **Web (MVC)** – Unified front end that surfaces data from all APIs and drives Google Drive upload workflows.
- **Executor (Scheduler)** – Aspire-managed Quartz scheduler that runs cron-based ingestion, refresh, and upload tasks.
### API Services
- **ComicCon.Api** – Ingests Comic Con guest data, persists it via EF Core, and provides searchable listings.
- **HumbleBundle.Api** – Fetches Humble Bundle metadata, builds archives, and hands them off to the uploader.
- **FileUploader.Api** – OAuth-secured Google Drive uploader supporting large resumable uploads.
- **Fitbit.Api** – Proxies Fitbit data retrieval and stores activity metrics.
- **Paissa.Api** – Surfaces FFXIV housing data for selected worlds.
### Data & Storage
- **SQL Server (EF Core)** – Primary persistence engine for the platform.
### External Services
- **Google Drive** – OAuth-protected destination for curated bundle uploads.

## Structure Diagram
```mermaid
flowchart TD

%% -----------------------------------------
%% FLAT UI STYLES
%% -----------------------------------------
classDef layer fill:#2b2b2b,stroke:#2b2b2b,color:#ffffff,rx:8px,ry:8px;

classDef aspire fill:#0096a7,stroke:#0096a7,color:#ffffff,rx:8px,ry:8px;
classDef web fill:#0d6efd,stroke:#0d6efd,color:#ffffff,rx:8px,ry:8px;
classDef scheduler fill:#b8860b,stroke:#b8860b,color:#ffffff,rx:8px,ry:8px;

classDef api fill:#1b8a5a,stroke:#1b8a5a,color:#ffffff,rx:8px,ry:8px;

classDef data fill:#6f42c1,stroke:#6f42c1,color:#ffffff,rx:8px,ry:8px;
classDef external fill:#c0392b,stroke:#c0392b,color:#ffffff,rx:8px,ry:8px;

%% -----------------------------------------
%% ASPIRE ORCHESTRATION
%% -----------------------------------------
subgraph Aspire["Aspire Orchestration"]
    AppHost["Aspire AppHost"]:::aspire
end
class Aspire layer;

%% -----------------------------------------
%% WEB LAYER
%% -----------------------------------------
subgraph WebLayer["Web Layer"]
    Web["Web MVC"]:::web
    Executor["Executor Scheduler"]:::scheduler
end
class WebLayer layer;

%% -----------------------------------------
%% API LAYER
%% -----------------------------------------
subgraph ApiLayer["API Services"]
    ComicConApi["ComicCon.Api"]:::api
    HumbleBundleApi["HumbleBundle.Api"]:::api
    PaissaApi["Paissa.Api"]:::api
    FitbitApi["Fitbit.Api"]:::api
    FileUploaderApi["FileUploader.Api"]:::api
end
class ApiLayer layer;

%% -----------------------------------------
%% DATA & EXTERNAL
%% -----------------------------------------
subgraph DataLayer["Data / Storage"]
    SQL["SQL Server (EF Core)"]:::data
end
class DataLayer layer;

subgraph ExternalLayer["External Services"]
    GoogleDrive["Google Drive (OAuth Uploads)"]:::external
end
class ExternalLayer layer;

%% -----------------------------------------
%% CONNECTIONS
%% -----------------------------------------
AppHost --> Web
AppHost --> Executor

Web --> ComicConApi
Web --> HumbleBundleApi
Web --> PaissaApi
Web --> FitbitApi
Web --> FileUploaderApi

Executor --> ComicConApi
Executor --> HumbleBundleApi
Executor --> PaissaApi

ComicConApi --> SQL
HumbleBundleApi --> SQL
FitbitApi --> SQL
PaissaApi --> SQL
FileUploaderApi --> GoogleDrive
```