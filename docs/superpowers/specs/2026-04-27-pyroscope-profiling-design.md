# Pyroscope Continuous Profiling — Design

**Date:** 2026-04-27
**Status:** Approved

## Goal

Add Pyroscope continuous profiling to the DoSomethingService, pushing all profile types (CPU, allocations, exceptions, lock contention) to the local Grafana Alloy instance, with trace-to-profile linking so a slow span in Grafana Explore can be clicked through to the matching CPU profile.

## Approach

Pyroscope .NET SDK (Option A): NuGet packages for configuration + OTel integration, native CLR profiler loaded via environment variables. Follows the existing `GrafanaServiceCollectionExtensions` pattern.

## Architecture

### Packages

| Package | Purpose |
|---|---|
| `Pyroscope` | Managed SDK — config API, push endpoint, labels |
| `Pyroscope.Otel` | OTel exporter — stamps `pyroscope.profile.id` onto spans |

The native CLR profiler ships inside the `Pyroscope` NuGet and is extracted to the output directory at build time. It must be loaded by the runtime via env vars before the process starts — no amount of code can load it after the fact.

### Components

**`GrafanaOptions` record** (`GrafanaServiceCollectionExtensions.cs`)
Add `string PyroscopeUrl` field. Example value: `http://localhost:4040` (Alloy's default Pyroscope receiver port).

**`GrafanaPyroscopeExtensions.cs`** (new file in `Configuration/Telemetry/`)
Single public extension method:
```csharp
services.AddGrafanaProfiling(configuration);
```
Reads `GrafanaOptions`, validates `PyroscopeUrl` is present (throws `InvalidOperationException` if missing), builds `PyroscopeConfig` with push URL, app name, and all profile type flags, then calls `Pyroscope.Sdk.Configure(config)`.

**`GrafanaServiceCollectionExtensions.cs`** (existing file)
Wire `.AddPyroscopeExporter()` into the existing OTel tracing builder so every span receives a `pyroscope.profile.id` attribute. This is the link that lets Grafana join a trace span to a profile snapshot.

**`Startup.cs`**
Call `services.AddGrafanaProfiling(Configuration)` immediately after `services.AddGrafanaTelemetry(Configuration)`.

### Configuration

`appsettings.json` — add `PyroscopeUrl` under existing `Grafana` section:
```json
"Grafana": {
  "Environment": "dev",
  "GrpcUrl": "http://localhost:4317",
  "HttpUrl": "http://localhost:4318",
  "ServiceName": "sample-service",
  "PyroscopeUrl": "http://localhost:4040"
}
```

`launchSettings.json` — add to both `http` and `https` profiles:
```json
"CORECLR_ENABLE_PROFILING": "1",
"CORECLR_PROFILER": "{B6024BDE-4B4D-4CF0-9B65-FB3A40A50869}",
"CORECLR_PROFILER_PATH": "Pyroscope.Profiler.Native.dylib",
"PYROSCOPE_PROFILING_ALLOCATION_ENABLED": "true",
"PYROSCOPE_PROFILING_EXCEPTION_ENABLED": "true",
"PYROSCOPE_PROFILING_LOCK_ENABLED": "true"
```

CPU profiling is on by default; the last three enable the remaining types. `CORECLR_PROFILER_PATH` is a relative path that works when `dotnet run` is invoked from the project directory — at runtime the CLR resolves it against the output directory where the NuGet extracts the native binary. For Linux deployments the filename is `Pyroscope.Profiler.Native.so`; for Windows, `Pyroscope.Profiler.Native.dll`.

## Data Flow

```
ASP.NET Core process
  │
  ├─ CLR profiler (native, loaded via env vars)
  │    Samples: CPU, alloc, exceptions, locks
  │    Pushes profiles → Alloy :4040
  │
  └─ Pyroscope.Otel exporter (managed, in OTel pipeline)
       Stamps pyroscope.profile.id on every span
       Spans exported → Alloy :4318 (existing OTLP)

Alloy
  ├─ Forwards profiles → Grafana Cloud Profiles
  └─ Forwards traces   → Grafana Cloud Tempo

Grafana Explore
  Trace span → click profile.id → matching CPU profile
```

## Testing

### Unit Tests (no CLR profiler required)

Added to existing `DoSomethingService.Tests` project under `Configuration/Telemetry/GrafanaPyroscopeExtensionsTests.cs`.

| Test | What it verifies |
|---|---|
| `AddGrafanaProfiling_MissingPyroscopeUrl_ThrowsInvalidOperationException` | Null/missing URL gives a clear error |
| `AddGrafanaProfiling_ConfiguresCorrectPushUrl` | Push URL matches config value |
| `AddGrafanaProfiling_ConfiguresCorrectAppName` | `ServiceName` from config is used as Pyroscope app name |
| `AddGrafanaProfiling_AllProfileTypesEnabled` | CPU, allocation, exception, lock flags all set to true |

### Integration Test (verifies data is actually pushed)

In `GrafanaPyroscopeIntegrationTests.cs`, marked `[Trait("Category", "Integration")]`.

1. Starts an in-process `HttpListener` on a free port
2. Calls `AddGrafanaProfiling` pointing at that listener
3. Exercises the service for a short period
4. Asserts at least one HTTP POST is received within 10 seconds

**Skip condition:** if `CORECLR_ENABLE_PROFILING` env var is absent the test calls `Skip.If(...)` (from `Xunit.SkippableFact` package, added to the test project) and exits cleanly rather than failing. This means CI passes without the native profiler, but the test is available for local verification and can be added to a dedicated profiler-enabled CI job later.

## Error Handling

- Missing `Grafana:PyroscopeUrl` → `InvalidOperationException` at startup (fail fast, same pattern as existing telemetry config)
- CLR profiler env vars absent at runtime → profiling silently disabled by the runtime; the managed SDK still configures but no samples are collected. Log a warning at startup if `CORECLR_ENABLE_PROFILING` is not set.

## Out of Scope

- Configuring Alloy to receive/forward profiles (Alloy-side config is a separate concern)
- Production deployment env vars for Azure Web App (documented separately)
- Dynamic profiling labels per-request (can be added later via `Pyroscope.LabelsWrapper`)
