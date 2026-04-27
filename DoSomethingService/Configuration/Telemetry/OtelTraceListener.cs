using System.Diagnostics.Tracing;
using Serilog;

namespace DoSomethingService.Configuration.Telemetry;

public class OtelTraceListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name.StartsWith("OpenTelemetry")) EnableEvents(eventSource, EventLevel.LogAlways);
        //base.OnEventSourceCreated(eventSource);
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventSource.Name.StartsWith("OpenTelemetry"))
        {
            Log.Debug(string.Format(eventData.Message!, eventData.Payload!.ToArray()));
        }
    }
}