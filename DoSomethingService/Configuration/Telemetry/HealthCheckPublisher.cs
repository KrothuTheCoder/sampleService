using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ILogger = Serilog.ILogger;

namespace DoSomethingService.Configuration.Telemetry;

public class HealthCheckPublisher : IHealthCheckPublisher, IDisposable
{
    private readonly Meter _meter;
    private readonly Gauge<int> _checkHealthGauge;
    private readonly Gauge<int> _overallHealthGauge;
    private readonly ILogger _logger;

    public HealthCheckPublisher(ILogger logger, ITelemetryContext telemetryContext)
    {
        _meter = new Meter("health.status");
        _logger = logger.ForContext<HealthCheckPublisher>();
        _overallHealthGauge = _meter.CreateGauge<int>(
            "overallHealth", description: "Overall health status 0 = unhealthy, 1 = degraded, 2 = healthy");
        _checkHealthGauge = _meter.CreateGauge<int>(
            "checkStatus", description: "Check status 0 = unhealthy, 1 = degraded, 2 = healthy");
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        _overallHealthGauge.Record(Map(report.Status));
        _logger.Information("Health check completed {status}", report.Status);
        foreach (var (name, entry) in report.Entries)
            _checkHealthGauge.Record(Map(entry.Status),
                new KeyValuePair<string, object?>("check.name", name));

        if (report.Status != HealthStatus.Healthy)
            _logger.Warning("Health check failed {status}, checks: {checks}", report.Status,
                string.Join(", ", report.Entries.Select(entry => $"{entry.Key}={entry.Value.Status}")));
        return Task.CompletedTask;
    }

    public void Dispose() => _meter.Dispose();

    private static int Map(HealthStatus status) => status switch
    {
        HealthStatus.Unhealthy => 0,
        HealthStatus.Degraded => 1,
        HealthStatus.Healthy => 2,
        _ => 0
    };
}