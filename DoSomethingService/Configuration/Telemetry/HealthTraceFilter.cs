using System.Diagnostics;
using OpenTelemetry;

namespace DoSomethingService.Configuration.Telemetry;

public class HealthTraceFilter : BaseProcessor<Activity>
{
    private static readonly string[] HealthCheckPaths = ["/health", "/ready", "/liveness"];

    public override void OnEnd(Activity data)
    {
        if (HealthCheckPaths.Any(path => data.DisplayName.Contains(path, StringComparison.OrdinalIgnoreCase)))
            data.ActivityTraceFlags = ActivityTraceFlags.None;

        base.OnEnd(data);
    }
}