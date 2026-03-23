using System.Net;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.OpenTelemetry;

namespace DoSomethingService.Telemetry;

public sealed record GrafanaOptions(
    string GrpcUrl,
    string HttpUrl,
    string ServiceName,
    string Environment);

public static class GrafanaServiceCollectionExtensions
{

    private static ResourceBuilder _appResourceBuilder;

    public static void AddGrafanaTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var grafanaOptions = configuration.GetSection("Grafana").Get<GrafanaOptions>();
        var telemetryContext = new TelemetryContext(grafanaOptions.ServiceName,"1.0");
        
        services.AddSingleton(telemetryContext);
        
        _appResourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: grafanaOptions.ServiceName);

        services.AddOpenTelemetry()
            .WithMetrics(metricOptions =>
            {
                Log.Information("Adding open telemetry for service {service} in env {env}", grafanaOptions.ServiceName, grafanaOptions.Environment);
                metricOptions
                    .AddAspNetCoreInstrumentation()
                    .SetResourceBuilder(_appResourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddProcessInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(grafanaOptions.GrpcUrl);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            })
            .WithTracing(tracingOptions =>
            {
                Log.Information("Adding tracing for service {service}", grafanaOptions.ServiceName, grafanaOptions.Environment);

                tracingOptions
                    .AddSource(grafanaOptions.ServiceName)
                    .SetResourceBuilder(_appResourceBuilder)
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(grafanaOptions.GrpcUrl);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            });
    }

    private static ResourceBuilder CreateAppResourceBuilder(GrafanaOptions grafanaOptions)
    {
        return ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: grafanaOptions.ServiceName,
                serviceVersion: "1.0.0",
                autoGenerateServiceInstanceId: false,
                serviceInstanceId: Dns.GetHostName())
            .AddAttributes([
                new KeyValuePair<string, object>("env", grafanaOptions.Environment),
                new KeyValuePair<string, object>("serviceName", grafanaOptions.ServiceName)
            ]);
    }

    public static LoggerConfiguration AddGrafanaLogging(this LoggerSinkConfiguration loggerSinkConfiguration,
        IConfiguration configuration,
        Action<OpenTelemetrySinkOptions> congifure = null)
    {
        ArgumentNullException.ThrowIfNull(loggerSinkConfiguration);
        ArgumentNullException.ThrowIfNull(configuration);

        var grafanaOptions = configuration.GetSection("Grafana").Get<GrafanaOptions>();
        
        Log.Information("Adding open telemetry log sink url: {LogUrl}", grafanaOptions.HttpUrl);

        return loggerSinkConfiguration.OpenTelemetry(options =>
        {
            options.LogsEndpoint = grafanaOptions.HttpUrl;
            options.Protocol = OtlpProtocol.Grpc;
            options.IncludedData =
                IncludedData.SpanIdField
                | IncludedData.TraceIdField
                | IncludedData.MessageTemplateTextAttribute
                | IncludedData.MessageTemplateMD5HashAttribute;
            options.ResourceAttributes = new Dictionary<string, object>
            {
                ["ServiceName"] = grafanaOptions.ServiceName,
                ["Environment"] = grafanaOptions.Environment,
                ["ServiceVersion"] = "1.0.0",
                ["InstanceId"] = Dns.GetHostName()
            };
            congifure?.Invoke(options);
        });
    }
}