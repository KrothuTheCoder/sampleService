# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet restore
dotnet build
dotnet run --project DoSomethingService
```

Development URLs: `http://localhost:5150` (HTTP) / `https://localhost:7036` (HTTPS)  
Swagger UI: `http://localhost:5150/swagger`

No test project currently exists in the solution.

## Tech Stack

- **.NET 10** ASP.NET Core Web API
- **Serilog** for structured logging with Grafana Loki sink
- **OpenTelemetry** (via `Grafana.OpenTelemetry`) for traces, metrics, and logs
- **Swashbuckle** for Swagger/OpenAPI docs
- **Microsoft.AspNetCore.Mvc.Versioning** for API versioning

## Architecture

### Startup Flow

`Program.cs` creates a `WebApplicationBuilder`, registers Serilog via `UseSerilog`, then calls `Startup.ConfigureServices` / `Startup.Configure`. All DI registration is in `Startup.cs`; the middleware pipeline is also defined there.

### API Versioning

Two controller versions live under `Controllers/v1/` and `Controllers/v2/`. Versioning is namespace-based via `ApiExplorerGroupPerVersionConvention` (see `Configuration/Swagger/`). Routes follow `/api/v{n}/something/...`. Swagger docs are split per version.

### Observability Stack

All telemetry bootstrapping is in `Configuration/Telemetry/`:

- `GrafanaServiceCollectionExtensions.cs` — registers OpenTelemetry with traces, metrics (ASP.NET Core, HTTP client, process), and logs. Exports via OTLP to the gRPC/HTTP endpoints in `appsettings.json` under the `Grafana` key.
- `OtelTraceListener.cs` — bridges `System.Diagnostics.Trace` to the OpenTelemetry `ActivitySource`.
- `HealthCheckPublisher.cs` — publishes health check results as Grafana metrics every 5 seconds.
- `HealthTraceFilter.cs` — excludes health check traffic from trace export.

### Health Checks

Mapped at `/health`, `/ready`, `/liveness` in `Startup.Configure`. `HealthCheckPublisher` converts results into OpenTelemetry metrics.

### Configuration

Key `appsettings.json` sections:
- `Grafana` — `GrpcUrl`, `HttpUrl`, `ServiceName`, `Environment` for OTLP export
- `Cors.AllowedOrigins` — list of allowed origins for the CORS policy

### Deployment

GitHub Actions (`.github/workflows/main_thedosomethingservice.yml`) builds on `push` to `main` and deploys to the Azure Web App `theDoSomethingService`. An older `azure-pipelines.yml` also exists but is likely inactive.
