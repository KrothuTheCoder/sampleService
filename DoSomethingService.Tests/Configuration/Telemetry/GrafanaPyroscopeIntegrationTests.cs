using System.Net;
using DoSomethingService.Configuration.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DoSomethingService.Tests.Configuration.Telemetry;

[Trait("Category", "Integration")]
public class GrafanaPyroscopeIntegrationTests
{
    [SkippableFact]
    public void AddGrafanaProfiling_WhenClrProfilerLoaded_SetsProfilerEnvironmentVariables()
    {
        Skip.If(
            Environment.GetEnvironmentVariable("CORECLR_ENABLE_PROFILING") != "1",
            "CLR profiler not loaded — set CORECLR_ENABLE_PROFILING=1 and restart the process to run this test.");

        var savedServerAddress = Environment.GetEnvironmentVariable("PYROSCOPE_SERVER_ADDRESS");
        var savedAppName = Environment.GetEnvironmentVariable("PYROSCOPE_APPLICATION_NAME");
        var savedAllocation = Environment.GetEnvironmentVariable("PYROSCOPE_PROFILING_ALLOCATION_ENABLED");
        var savedException = Environment.GetEnvironmentVariable("PYROSCOPE_PROFILING_EXCEPTION_ENABLED");
        var savedLock = Environment.GetEnvironmentVariable("PYROSCOPE_PROFILING_LOCK_ENABLED");

        try
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Grafana:ServiceName"] = "integration-test",
                    ["Grafana:Environment"] = "test",
                    ["Grafana:HttpUrl"] = "http://localhost:4318",
                    ["Grafana:GrpcUrl"] = "http://localhost:4317",
                    ["Grafana:PyroscopeUrl"] = "http://localhost:4040"
                })
                .Build();

            new ServiceCollection().AddGrafanaProfiling(config);

            Assert.Equal("http://localhost:4040", Environment.GetEnvironmentVariable("PYROSCOPE_SERVER_ADDRESS"));
            Assert.Equal("integration-test", Environment.GetEnvironmentVariable("PYROSCOPE_APPLICATION_NAME"));
            Assert.Equal("true", Environment.GetEnvironmentVariable("PYROSCOPE_PROFILING_ALLOCATION_ENABLED"));
            Assert.Equal("true", Environment.GetEnvironmentVariable("PYROSCOPE_PROFILING_EXCEPTION_ENABLED"));
            Assert.Equal("true", Environment.GetEnvironmentVariable("PYROSCOPE_PROFILING_LOCK_ENABLED"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PYROSCOPE_SERVER_ADDRESS", savedServerAddress);
            Environment.SetEnvironmentVariable("PYROSCOPE_APPLICATION_NAME", savedAppName);
            Environment.SetEnvironmentVariable("PYROSCOPE_PROFILING_ALLOCATION_ENABLED", savedAllocation);
            Environment.SetEnvironmentVariable("PYROSCOPE_PROFILING_EXCEPTION_ENABLED", savedException);
            Environment.SetEnvironmentVariable("PYROSCOPE_PROFILING_LOCK_ENABLED", savedLock);
        }
    }
}
