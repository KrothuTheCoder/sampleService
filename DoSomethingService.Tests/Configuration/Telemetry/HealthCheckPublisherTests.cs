using System.Diagnostics.Metrics;
using DoSomethingService.Configuration.Telemetry;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace DoSomethingService.Tests.Configuration.Telemetry;

[Collection("HealthCheckPublisher")]
public class HealthCheckPublisherTests
{
    private static HealthCheckPublisher CreatePublisher() =>
        new(new LoggerConfiguration().CreateLogger(), new TelemetryContext("test-svc", "1.0"));

    private static HealthReport MakeReport(HealthStatus status) =>
        new(new Dictionary<string, HealthReportEntry>(), status, TimeSpan.Zero);

    private static HealthReport MakeReportWithCheck(string checkName, HealthStatus checkStatus) =>
        new(
            new Dictionary<string, HealthReportEntry>
            {
                [checkName] = new(checkStatus, checkName, TimeSpan.Zero, null, null)
            },
            checkStatus,
            TimeSpan.Zero);

    [Theory]
    [InlineData(HealthStatus.Healthy, 2)]
    [InlineData(HealthStatus.Degraded, 1)]
    [InlineData(HealthStatus.Unhealthy, 0)]
    public async Task PublishAsync_RecordsCorrectOverallHealthGaugeValue(HealthStatus status, int expected)
    {
        var recorded = new List<int>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Name == "overallHealth") l.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<int>((instrument, value, _, _) =>
        {
            if (instrument.Name == "overallHealth") recorded.Add(value);
        });
        listener.Start();

        var publisher = CreatePublisher();
        await publisher.PublishAsync(MakeReport(status), CancellationToken.None);

        Assert.Contains(expected, recorded);
    }

    [Theory]
    [InlineData(HealthStatus.Healthy, 2)]
    [InlineData(HealthStatus.Degraded, 1)]
    [InlineData(HealthStatus.Unhealthy, 0)]
    public async Task PublishAsync_RecordsCorrectCheckStatusGaugeValue(HealthStatus status, int expected)
    {
        var recorded = new List<int>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Name == "checkStatus") l.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<int>((instrument, value, _, _) =>
        {
            if (instrument.Name == "checkStatus") recorded.Add(value);
        });
        listener.Start();

        var publisher = CreatePublisher();
        await publisher.PublishAsync(MakeReportWithCheck("self", status), CancellationToken.None);

        Assert.Contains(expected, recorded);
    }

    [Fact]
    public void CheckStatusGauge_Description_ReflectsCorrectMapping()
    {
        string? description = null;
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, _) =>
        {
            if (instrument.Name == "checkStatus") description = instrument.Description;
        };
        listener.Start();

        _ = CreatePublisher();

        Assert.Equal("Check status 0 = unhealthy, 1 = degraded, 2 = healthy", description);
    }

    [Fact]
    public void HealthCheckPublisher_ImplementsIDisposable()
    {
        var publisher = CreatePublisher();
        Assert.IsAssignableFrom<IDisposable>(publisher);
    }
}
