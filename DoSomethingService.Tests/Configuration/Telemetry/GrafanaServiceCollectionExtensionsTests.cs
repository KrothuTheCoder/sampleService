using DoSomethingService.Configuration.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace DoSomethingService.Tests.Configuration.Telemetry;

public class GrafanaServiceCollectionExtensionsTests
{
    private static IConfiguration BuildGrafanaConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Grafana:ServiceName"] = "test-service",
                ["Grafana:Environment"] = "test",
                ["Grafana:HttpUrl"] = "http://localhost:4318",
                ["Grafana:GrpcUrl"] = "http://localhost:4317"
            })
            .Build();

    [Fact]
    public void AddGrafanaTelemetry_MissingGrafanaSection_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        Assert.Throws<InvalidOperationException>(() => services.AddGrafanaTelemetry(config));
    }

    [Fact]
    public void AddGrafanaLogging_UsesOtelSemanticConventionKeys()
    {
        var config = BuildGrafanaConfig();
        OpenTelemetrySinkOptions? captured = null;

        new LoggerConfiguration()
            .WriteTo.AddGrafanaLogging(config, opts => captured = opts)
            .CreateLogger();

        Assert.NotNull(captured);
        Assert.True(captured.ResourceAttributes.ContainsKey("service.name"),
            "Expected OTel semantic key 'service.name' but found: " +
            string.Join(", ", captured.ResourceAttributes.Keys));
        Assert.True(captured.ResourceAttributes.ContainsKey("service.version"),
            "Expected OTel semantic key 'service.version'");
        Assert.True(captured.ResourceAttributes.ContainsKey("deployment.environment"),
            "Expected OTel semantic key 'deployment.environment'");
        Assert.True(captured.ResourceAttributes.ContainsKey("service.instance.id"),
            "Expected OTel semantic key 'service.instance.id'");
    }

    [Fact]
    public void AddGrafanaLogging_ResourceAttributeValues_MatchConfig()
    {
        var config = BuildGrafanaConfig();
        OpenTelemetrySinkOptions? captured = null;

        new LoggerConfiguration()
            .WriteTo.AddGrafanaLogging(config, opts => captured = opts)
            .CreateLogger();

        Assert.NotNull(captured);
        Assert.Equal("test-service", captured.ResourceAttributes["service.name"]);
        Assert.Equal("test", captured.ResourceAttributes["deployment.environment"]);
    }
}
