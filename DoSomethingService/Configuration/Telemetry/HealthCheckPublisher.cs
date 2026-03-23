using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DoSomethingService.Configuration.Telemetry;

public class HealthCheckPublisher : IHealthCheckPublisher
{
    private readonly Gauge<int> _checkHealthGauge;
    private readonly Gauge<int> _overallHealthGauge;
    private ILogger<HealthCheckPublisher> _logger;

    public HealthCheckPublisher(ILogger<HealthCheckPublisher> logger, ITelemetryContext telemetryContext)
    {
        var healthMeter = new Meter("health.status");

        _overallHealthGauge = healthMeter.CreateGauge<int>(
            "overallHealth", description: "Overall health status 0 = unhealthy, 1 = degraded, 2 = healthy");
        _checkHealthGauge = healthMeter.CreateGauge<int>(
            "checkStatus", description: "Check status 0 = healthy, 1 = degraded, 2 = healthy");
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        _overallHealthGauge.Record(Map(report.Status));

        foreach (var (name, entry) in report.Entries)
            _checkHealthGauge.Record(Map(entry.Status),
                new KeyValuePair<string, object?>("check.name", name));

        if (report.Status != HealthStatus.Healthy)
            _logger.LogWarning("Health check failed {status}, checks: {checks}", report.Status,
                string.Join(", ", report.Entries.Select(entry => $"{entry.Key}={entry.Value.Status}")));
        return Task.CompletedTask;
    }

    private static int Map(HealthStatus status)
    {
        return status switch
        {
            HealthStatus.Unhealthy => 0,
            HealthStatus.Degraded => 1,
            HealthStatus.Healthy => 2,
            _ => 0
        };
    }
}