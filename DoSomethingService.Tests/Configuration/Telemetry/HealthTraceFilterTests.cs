using System.Diagnostics;
using DoSomethingService.Configuration.Telemetry;

namespace DoSomethingService.Tests.Configuration.Telemetry;

public class HealthTraceFilterTests
{
    [Theory]
    [InlineData("GET /health")]
    [InlineData("GET /ready")]
    [InlineData("GET /liveness")]
    public void OnEnd_ActivityContainingHealth_ClearsTraceFlags(string displayName)
    {
        var activity = new Activity(displayName);
        activity.ActivityTraceFlags = ActivityTraceFlags.Recorded;

        new HealthTraceFilter().OnEnd(activity);

        Assert.Equal(ActivityTraceFlags.None, activity.ActivityTraceFlags);
    }

    [Theory]
    [InlineData("GET /api/v1/something")]
    [InlineData("POST /api/v1/something/action")]
    public void OnEnd_ActivityNotContainingHealth_PreservesTraceFlags(string displayName)
    {
        var activity = new Activity(displayName);
        activity.ActivityTraceFlags = ActivityTraceFlags.Recorded;

        new HealthTraceFilter().OnEnd(activity);

        Assert.Equal(ActivityTraceFlags.Recorded, activity.ActivityTraceFlags);
    }
}
