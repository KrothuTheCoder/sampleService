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
