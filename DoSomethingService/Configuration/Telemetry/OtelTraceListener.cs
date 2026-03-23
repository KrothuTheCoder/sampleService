using System.Diagnostics.Tracing;

namespace DoSomethingService.Configuration.Telemetry;

public class OtelTraceListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name.StartsWith("OpenTelemetry")) EnableEvents(eventSource, EventLevel.LogAlways);
        base.OnEventSourceCreated(eventSource);
    }
}