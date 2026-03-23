using System.Diagnostics;

namespace DoSomethingService.Telemetry;

interface ITelemetryContext
{
    ActivitySource ActivitySource { get; }
}

public class TelemetryContext: ITelemetryContext
{
    public ActivitySource ActivitySource { get; }
    
    public TelemetryContext(string serviceName, string serviceVersion){
        ArgumentNullException.ThrowIfNull(serviceName);
        ArgumentNullException.ThrowIfNull(serviceVersion);
        ActivitySource = new ActivitySource(serviceName, serviceVersion);
    }
}