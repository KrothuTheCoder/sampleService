using System.Diagnostics;

namespace DoSomethingService.Configuration.Telemetry;

public interface ITelemetryContext
{
    ActivitySource ActivitySource { get; }
}

public class TelemetryContext : ITelemetryContext
{
    public TelemetryContext(string serviceName, string serviceVersion)
    {
        ActivitySource = new ActivitySource(serviceName, serviceVersion);
    }

    public ActivitySource ActivitySource { get; }
}