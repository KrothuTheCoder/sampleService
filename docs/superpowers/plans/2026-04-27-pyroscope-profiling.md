# Pyroscope Continuous Profiling Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Pyroscope continuous profiling (CPU, allocations, exceptions, locks) to DoSomethingService, pushing to local Grafana Alloy with trace-to-profile linking via OTel span attributes.

**Architecture:** The native CLR profiler (shipped inside the `Pyroscope` NuGet) is loaded via env vars before process start. A new `AddGrafanaProfiling` extension method — matching the existing telemetry pattern — validates config, sets profiler env vars, and wires `AddPyroscopeExporter()` into the OTel tracing pipeline so every span carries `pyroscope.profile.id`. Unit tests verify config via a capture callback. An integration test (skippable when the CLR profiler is absent) verifies actual HTTP pushes arrive.

**Tech Stack:** `Pyroscope` (NuGet), `Pyroscope.Otel` (NuGet), `Xunit.SkippableFact` (test NuGet), .NET 10, xunit 2.9.2, existing OpenTelemetry pipeline.

---

## File Map

| Action | Path | Responsibility |
|---|---|---|
| Modify | `DoSomethingService/DoSomethingService.csproj` | Add `Pyroscope` + `Pyroscope.Otel` packages |
| Modify | `DoSomethingService/Configuration/Telemetry/GrafanaServiceCollectionExtensions.cs` | Add `PyroscopeUrl` to `GrafanaOptions` record |
| **Create** | `DoSomethingService/Configuration/Telemetry/GrafanaPyroscopeExtensions.cs` | `AddGrafanaProfiling` extension + `GrafanaProfilingOptions` record |
| Modify | `DoSomethingService/Startup.cs` | Call `services.AddGrafanaProfiling(Configuration)` |
| Modify | `DoSomethingService/appsettings.json` | Add `"PyroscopeUrl": "http://localhost:4040"` under `Grafana` |
| Modify | `DoSomethingService/Properties/launchSettings.json` | Add CLR profiler + profile-type env vars |
| Modify | `DoSomethingService.Tests/DoSomethingService.Tests.csproj` | Add `Xunit.SkippableFact` package |
| Modify | `DoSomethingService.Tests/Configuration/Telemetry/GrafanaServiceCollectionExtensionsTests.cs` | Add `PyroscopeUrl` to `BuildGrafanaConfig()` helper |
| **Create** | `DoSomethingService.Tests/Configuration/Telemetry/GrafanaPyroscopeExtensionsTests.cs` | Unit tests for `AddGrafanaProfiling` |
| **Create** | `DoSomethingService.Tests/Configuration/Telemetry/GrafanaPyroscopeIntegrationTests.cs` | Integration test — verifies HTTP push arrives |

---

## Task 1: Add NuGet packages

**Files:**
- Modify: `DoSomethingService/DoSomethingService.csproj`
- Modify: `DoSomethingService.Tests/DoSomethingService.Tests.csproj`

- [ ] **Step 1: Add Pyroscope packages to production project**

In `DoSomethingService/DoSomethingService.csproj`, add inside the first `<ItemGroup>` (with the other PackageReferences):

```xml
<PackageReference Include="Pyroscope" Version="0.10.2"/>
<PackageReference Include="Pyroscope.Otel" Version="0.10.2"/>
```

> **Verify version before adding:** Run `dotnet add package Pyroscope` to let NuGet resolve the latest stable version, then copy the resolved version into the csproj. Do the same for `Pyroscope.Otel`. Both packages must be the same version.

- [ ] **Step 2: Add SkippableFact to test project**

In `DoSomethingService.Tests/DoSomethingService.Tests.csproj`, add inside the `<ItemGroup>` with the other test packages:

```xml
<PackageReference Include="Xunit.SkippableFact" Version="1.4.13" />
```

- [ ] **Step 3: Restore and verify**

```bash
dotnet restore
```

Expected: restore succeeds with no errors. Warnings about known vulnerabilities in transitive OTel packages are pre-existing and can be ignored.

- [ ] **Step 4: Commit**

```bash
git add DoSomethingService/DoSomethingService.csproj DoSomethingService.Tests/DoSomethingService.Tests.csproj
git commit -m "feat: add Pyroscope and Xunit.SkippableFact packages"
```

---

## Task 2: Extend GrafanaOptions + update existing test helper

**Files:**
- Modify: `DoSomethingService/Configuration/Telemetry/GrafanaServiceCollectionExtensions.cs:13-17`
- Modify: `DoSomethingService.Tests/Configuration/Telemetry/GrafanaServiceCollectionExtensionsTests.cs:11-20`

- [ ] **Step 1: Add `PyroscopeUrl` to `GrafanaOptions` record**

In `GrafanaServiceCollectionExtensions.cs`, update the record (line 13):

```csharp
public sealed record GrafanaOptions(
    string GrpcUrl,
    string HttpUrl,
    string ServiceName,
    string Environment,
    string? PyroscopeUrl);
```

`PyroscopeUrl` is nullable here because `AddGrafanaTelemetry` doesn't need it — only `AddGrafanaProfiling` validates it. Config binding fills it by name; existing callers that don't supply it get `null`.

- [ ] **Step 2: Update `BuildGrafanaConfig()` in existing tests**

In `GrafanaServiceCollectionExtensionsTests.cs`, update the helper to include `PyroscopeUrl` (prevents the binding from silently leaving it null when full config is expected):

```csharp
private static IConfiguration BuildGrafanaConfig() =>
    new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Grafana:ServiceName"] = "test-service",
            ["Grafana:Environment"] = "test",
            ["Grafana:HttpUrl"] = "http://localhost:4318",
            ["Grafana:GrpcUrl"] = "http://localhost:4317",
            ["Grafana:PyroscopeUrl"] = "http://localhost:4040"
        })
        .Build();
```

- [ ] **Step 3: Run existing tests to confirm still passing**

```bash
dotnet test DoSomethingService.Tests/DoSomethingService.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 16`

- [ ] **Step 4: Commit**

```bash
git add DoSomethingService/Configuration/Telemetry/GrafanaServiceCollectionExtensions.cs \
        DoSomethingService.Tests/Configuration/Telemetry/GrafanaServiceCollectionExtensionsTests.cs
git commit -m "feat: add PyroscopeUrl to GrafanaOptions"
```

---

## Task 3: Write failing unit tests for AddGrafanaProfiling

**Files:**
- Create: `DoSomethingService.Tests/Configuration/Telemetry/GrafanaPyroscopeExtensionsTests.cs`

- [ ] **Step 1: Create the test file**

Create `DoSomethingService.Tests/Configuration/Telemetry/GrafanaPyroscopeExtensionsTests.cs`:

```csharp
using DoSomethingService.Configuration.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DoSomethingService.Tests.Configuration.Telemetry;

public class GrafanaPyroscopeExtensionsTests
{
    private static IConfiguration BuildConfig(string? pyroscopeUrl = "http://localhost:4040") =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Grafana:ServiceName"] = "test-service",
                ["Grafana:Environment"] = "test",
                ["Grafana:HttpUrl"] = "http://localhost:4318",
                ["Grafana:GrpcUrl"] = "http://localhost:4317",
                ["Grafana:PyroscopeUrl"] = pyroscopeUrl
            })
            .Build();

    [Fact]
    public void AddGrafanaProfiling_MissingPyroscopeUrl_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var config = BuildConfig(pyroscopeUrl: null);

        Assert.Throws<InvalidOperationException>(() => services.AddGrafanaProfiling(config));
    }

    [Fact]
    public void AddGrafanaProfiling_ConfiguresCorrectPushUrl()
    {
        GrafanaProfilingOptions? captured = null;

        new ServiceCollection().AddGrafanaProfiling(BuildConfig(), opts => captured = opts);

        Assert.NotNull(captured);
        Assert.Equal("http://localhost:4040", captured.PyroscopeUrl);
    }

    [Fact]
    public void AddGrafanaProfiling_ConfiguresCorrectAppName()
    {
        GrafanaProfilingOptions? captured = null;

        new ServiceCollection().AddGrafanaProfiling(BuildConfig(), opts => captured = opts);

        Assert.NotNull(captured);
        Assert.Equal("test-service", captured.ApplicationName);
    }

    [Fact]
    public void AddGrafanaProfiling_AllProfileTypesEnabled()
    {
        GrafanaProfilingOptions? captured = null;

        new ServiceCollection().AddGrafanaProfiling(BuildConfig(), opts => captured = opts);

        Assert.NotNull(captured);
        Assert.True(captured.AllocationProfilingEnabled);
        Assert.True(captured.ExceptionProfilingEnabled);
        Assert.True(captured.LockProfilingEnabled);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test DoSomethingService.Tests/DoSomethingService.Tests.csproj
```

Expected: build error — `AddGrafanaProfiling` and `GrafanaProfilingOptions` do not exist yet. This is the RED state.

---

## Task 4: Implement GrafanaPyroscopeExtensions.cs

**Files:**
- Create: `DoSomethingService/Configuration/Telemetry/GrafanaPyroscopeExtensions.cs`

- [ ] **Step 1: Create the implementation**

Create `DoSomethingService/Configuration/Telemetry/GrafanaPyroscopeExtensions.cs`:

```csharp
using Pyroscope.OpenTelemetry; // verify namespace: check the Pyroscope.Otel package's public types after restore
using Serilog;

namespace DoSomethingService.Configuration.Telemetry;

public record GrafanaProfilingOptions(
    string PyroscopeUrl,
    string ApplicationName,
    bool AllocationProfilingEnabled = true,
    bool ExceptionProfilingEnabled = true,
    bool LockProfilingEnabled = true);

public static class GrafanaPyroscopeExtensions
{
    public static void AddGrafanaProfiling(this IServiceCollection services,
        IConfiguration configuration,
        Action<GrafanaProfilingOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var grafanaOptions = configuration.GetSection("Grafana").Get<GrafanaOptions>()
            ?? throw new InvalidOperationException("Missing required configuration section 'Grafana'.");

        if (string.IsNullOrWhiteSpace(grafanaOptions.PyroscopeUrl))
            throw new InvalidOperationException("Missing required configuration value 'Grafana:PyroscopeUrl'.");

        if (Environment.GetEnvironmentVariable("CORECLR_ENABLE_PROFILING") != "1")
            Log.Warning("CORECLR_ENABLE_PROFILING is not set to 1 — " +
                        "native CLR profiler will not collect samples. " +
                        "Add it to launchSettings.json or deployment environment variables.");

        var options = new GrafanaProfilingOptions(
            grafanaOptions.PyroscopeUrl,
            grafanaOptions.ServiceName);

        configure?.Invoke(options);

        Environment.SetEnvironmentVariable("PYROSCOPE_SERVER_ADDRESS", options.PyroscopeUrl);
        Environment.SetEnvironmentVariable("PYROSCOPE_APPLICATION_NAME", options.ApplicationName);
        Environment.SetEnvironmentVariable("PYROSCOPE_PROFILING_ALLOCATION_ENABLED",
            options.AllocationProfilingEnabled ? "true" : "false");
        Environment.SetEnvironmentVariable("PYROSCOPE_PROFILING_EXCEPTION_ENABLED",
            options.ExceptionProfilingEnabled ? "true" : "false");
        Environment.SetEnvironmentVariable("PYROSCOPE_PROFILING_LOCK_ENABLED",
            options.LockProfilingEnabled ? "true" : "false");

        Log.Information("Configuring Pyroscope profiling for {service} → {url}",
            options.ApplicationName, options.PyroscopeUrl);

        services.ConfigureOpenTelemetryTracerProvider(b => b.AddPyroscopeExporter());
    }
}
```

- [ ] **Step 2: Run unit tests to verify they pass**

```bash
dotnet test DoSomethingService.Tests/DoSomethingService.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 20` (16 existing + 4 new).

- [ ] **Step 3: Commit**

```bash
git add DoSomethingService/Configuration/Telemetry/GrafanaPyroscopeExtensions.cs \
        DoSomethingService.Tests/Configuration/Telemetry/GrafanaPyroscopeExtensionsTests.cs
git commit -m "feat: implement AddGrafanaProfiling with unit tests"
```

---

## Task 5: Wire into Startup and update config files

**Files:**
- Modify: `DoSomethingService/Startup.cs:34`
- Modify: `DoSomethingService/appsettings.json`
- Modify: `DoSomethingService/Properties/launchSettings.json`

- [ ] **Step 1: Call AddGrafanaProfiling in Startup.cs**

In `Startup.cs`, add the call immediately after `services.AddGrafanaTelemetry(Configuration)` (currently line 34):

```csharp
services.AddGrafanaTelemetry(Configuration);
services.AddGrafanaProfiling(Configuration);
```

- [ ] **Step 2: Add PyroscopeUrl to appsettings.json**

Replace the `Grafana` section in `DoSomethingService/appsettings.json`:

```json
"Grafana": {
  "Environment": "dev",
  "GrpcUrl": "http://localhost:4317",
  "HttpUrl": "http://localhost:4318",
  "ServiceName": "sample-service",
  "PyroscopeUrl": "http://localhost:4040"
},
```

- [ ] **Step 3: Add CLR profiler env vars to launchSettings.json**

Add to `environmentVariables` in BOTH the `http` and `https` profiles in `DoSomethingService/Properties/launchSettings.json`:

```json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development",
  "CORECLR_ENABLE_PROFILING": "1",
  "CORECLR_PROFILER": "{B6024BDE-4B4D-4CF0-9B65-FB3A40A50869}",
  "CORECLR_PROFILER_PATH": "Pyroscope.Profiler.Native.dylib",
  "PYROSCOPE_PROFILING_ALLOCATION_ENABLED": "true",
  "PYROSCOPE_PROFILING_EXCEPTION_ENABLED": "true",
  "PYROSCOPE_PROFILING_LOCK_ENABLED": "true"
}
```

> **Note:** `CORECLR_PROFILER_PATH` uses the macOS filename. On Linux change to `Pyroscope.Profiler.Native.so`; on Windows `Pyroscope.Profiler.Native.dll`. The path is relative — the CLR resolves it from the output directory where the NuGet extracts the native binary at build time.

- [ ] **Step 4: Build to catch any integration issues**

```bash
dotnet build DoSomethingService/DoSomethingService.csproj
```

Expected: Build succeeded with 0 errors.

- [ ] **Step 5: Run full test suite**

```bash
dotnet test DoSomethingService.Tests/DoSomethingService.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 20`

- [ ] **Step 6: Commit**

```bash
git add DoSomethingService/Startup.cs \
        DoSomethingService/appsettings.json \
        DoSomethingService/Properties/launchSettings.json
git commit -m "feat: wire Pyroscope profiling into startup and config"
```

---

## Task 6: Write integration test

**Files:**
- Create: `DoSomethingService.Tests/Configuration/Telemetry/GrafanaPyroscopeIntegrationTests.cs`

The integration test starts an in-process HTTP listener, points Pyroscope at it, burns CPU to generate samples, and asserts a push request arrives. It uses `[SkippableFact]` from `Xunit.SkippableFact` — if `CORECLR_ENABLE_PROFILING` is absent the test skips cleanly instead of failing, so CI passes without the native profiler.

- [ ] **Step 1: Create the integration test file**

Create `DoSomethingService.Tests/Configuration/Telemetry/GrafanaPyroscopeIntegrationTests.cs`:

```csharp
using System.Diagnostics;
using System.Net;
using DoSomethingService.Configuration.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DoSomethingService.Tests.Configuration.Telemetry;

[Trait("Category", "Integration")]
public class GrafanaPyroscopeIntegrationTests
{
    [SkippableFact]
    public async Task AddGrafanaProfiling_WhenClrProfilerLoaded_PushesProfilesToServer()
    {
        Skip.If(
            Environment.GetEnvironmentVariable("CORECLR_ENABLE_PROFILING") != "1",
            "CLR profiler not loaded — set CORECLR_ENABLE_PROFILING=1 and restart the process to run this test.");

        // Find a free port
        int port;
        using (var tmp = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0))
        {
            tmp.Start();
            port = ((IPEndPoint)tmp.LocalEndpoint).Port;
        }

        var received = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://localhost:{port}/");
        httpListener.Start();

        // Accept any incoming request in background
        _ = Task.Run(async () =>
        {
            try
            {
                while (httpListener.IsListening)
                {
                    var ctx = await httpListener.GetContextAsync();
                    ctx.Response.StatusCode = 200;
                    ctx.Response.Close();
                    received.TrySetResult(true);
                }
            }
            catch (HttpListenerException) { /* listener stopped */ }
        });

        try
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Grafana:ServiceName"] = "integration-test",
                    ["Grafana:Environment"] = "test",
                    ["Grafana:HttpUrl"] = "http://localhost:4318",
                    ["Grafana:GrpcUrl"] = "http://localhost:4317",
                    ["Grafana:PyroscopeUrl"] = $"http://localhost:{port}"
                })
                .Build();

            new ServiceCollection().AddGrafanaProfiling(config);

            // Burn CPU to produce sample data
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < TimeSpan.FromSeconds(3))
                _ = Enumerable.Range(0, 10_000).Sum();

            // Wait up to 10 seconds for a push
            var completed = await Task.WhenAny(received.Task, Task.Delay(TimeSpan.FromSeconds(10)));
            Assert.True(completed == received.Task,
                "Expected at least one profile push to arrive within 10 seconds. " +
                "Verify PYROSCOPE_SERVER_ADDRESS was not already set before this process started.");
        }
        finally
        {
            httpListener.Stop();
            httpListener.Close();
        }
    }
}
```

- [ ] **Step 2: Run full test suite — integration test should skip**

```bash
dotnet test DoSomethingService.Tests/DoSomethingService.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 20, Skipped: 1` — the integration test skips because `CORECLR_ENABLE_PROFILING` is not set in this shell.

- [ ] **Step 3: Commit**

```bash
git add DoSomethingService.Tests/Configuration/Telemetry/GrafanaPyroscopeIntegrationTests.cs
git commit -m "feat: add Pyroscope integration test (skippable without CLR profiler)"
```

---

## Task 7: Final verification

- [ ] **Step 1: Run full test suite clean**

```bash
dotnet test DoSomethingService.Tests/DoSomethingService.Tests.csproj --verbosity normal
```

Expected output (approximate):
```
Passed!  - Failed: 0, Passed: 20, Skipped: 1, Total: 21
```

- [ ] **Step 2: Build production project clean**

```bash
dotnet build DoSomethingService/DoSomethingService.csproj --no-incremental
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 3: Verify native profiler binary was extracted (macOS)**

```bash
find DoSomethingService/bin -name "Pyroscope.Profiler.Native.dylib" 2>/dev/null
```

Expected: at least one path printed, e.g. `DoSomethingService/bin/Debug/net10.0/Pyroscope.Profiler.Native.dylib`. If empty, the package version in Task 1 may need adjusting.

- [ ] **Step 4: Final commit**

```bash
git add -A
git commit -m "feat: add Pyroscope continuous profiling with trace-profile linking"
```
