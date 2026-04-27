using System.Net;
using System.Reflection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.OpenTelemetry;

namespace DoSomethingService.Configuration.Telemetry;

public sealed record GrafanaOptions(
    string GrpcUrl,
    string HttpUrl,
    string ServiceName,
    string Environment,
    string? PyroscopeUrl);

public static class GrafanaServiceCollectionExtensions
{
    public static void AddGrafanaTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var grafanaOptions = configuration.GetSection("Grafana").Get<GrafanaOptions>()
            ?? throw new InvalidOperationException("Missing required configuration section 'Grafana'.");

        var telemetryContext = new TelemetryContext(grafanaOptions.ServiceName, ServiceVersion);
        services.AddSingleton<ITelemetryContext>(telemetryContext);

        var resourceBuilder = BuildResourceBuilder(grafanaOptions);

        services.AddOpenTelemetry()
            .WithMetrics(metricOptions =>
            {
                Log.Information("Adding open telemetry for service {service} in env {env}",
                    grafanaOptions.ServiceName, grafanaOptions.Environment);
                metricOptions
                    .AddAspNetCoreInstrumentation()
                    .SetResourceBuilder(resourceBuilder)
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddProcessInstrumentation()
                    .AddMeter("health.status")
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri($"{grafanaOptions.HttpUrl}/v1/metrics");
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
            })
            .WithTracing(tracingOptions =>
            {
                Log.Information("Adding tracing for service {service} in env {env}",
                    grafanaOptions.ServiceName, grafanaOptions.Environment);

                tracingOptions
                    .AddSource(grafanaOptions.ServiceName)
                    .SetResourceBuilder(resourceBuilder)
                    .AddProcessor<HealthTraceFilter>()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri($"{grafanaOptions.HttpUrl}/v1/traces");
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
            });
    }

    public static LoggerConfiguration AddGrafanaLogging(this LoggerSinkConfiguration loggerSinkConfiguration,
        IConfiguration configuration,
        Action<OpenTelemetrySinkOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(loggerSinkConfiguration);
        ArgumentNullException.ThrowIfNull(configuration);

        var grafanaOptions = configuration.GetSection("Grafana").Get<GrafanaOptions>()
            ?? throw new InvalidOperationException("Missing required configuration section 'Grafana'.");

        Log.Information("Adding open telemetry log sink url: {LogUrl}", grafanaOptions.HttpUrl);

        return loggerSinkConfiguration.OpenTelemetry(options =>
        {
            options.LogsEndpoint = $"{grafanaOptions.HttpUrl}/v1/logs";
            options.Protocol = OtlpProtocol.HttpProtobuf;
            options.IncludedData =
                IncludedData.SpanIdField
                | IncludedData.TraceIdField
                | IncludedData.MessageTemplateTextAttribute
                | IncludedData.MessageTemplateMD5HashAttribute;
            options.ResourceAttributes = new Dictionary<string, object>
            {
                ["service.name"] = grafanaOptions.ServiceName,
                ["deployment.environment"] = grafanaOptions.Environment,
                ["service.version"] = ServiceVersion,
                ["service.instance.id"] = Dns.GetHostName()
            };
            configure?.Invoke(options);
        });
    }

    private static string ServiceVersion =>
        Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "1.0.0";

    private static ResourceBuilder BuildResourceBuilder(GrafanaOptions grafanaOptions) =>
        ResourceBuilder.CreateDefault()
            .AddService(
                grafanaOptions.ServiceName,
                serviceVersion: ServiceVersion,
                autoGenerateServiceInstanceId: false,
                serviceInstanceId: Dns.GetHostName())
            .AddAttributes([
                new KeyValuePair<string, object>("deployment.environment", grafanaOptions.Environment)
            ]);
}