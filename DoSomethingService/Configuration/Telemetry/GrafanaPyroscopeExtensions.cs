using OpenTelemetry.Trace;
using Serilog;

namespace DoSomethingService.Configuration.Telemetry;

public sealed record GrafanaProfilingOptions
{
    public GrafanaProfilingOptions(string pyroscopeUrl, string applicationName, string? basicAuthUser = null)
    {
        PyroscopeUrl = pyroscopeUrl;
        ApplicationName = applicationName;
        BasicAuthUser = basicAuthUser;
    }

    public string PyroscopeUrl { get; init; }
    public string ApplicationName { get; init; }
    public string? BasicAuthUser { get; set; }
    public bool AllocationProfilingEnabled { get; set; } = true;
    public bool ExceptionProfilingEnabled { get; set; } = true;
    public bool LockProfilingEnabled { get; set; } = true;
}

public static class GrafanaPyroscopeExtensions
{
    public static void AddGrafanaProfiling(this IServiceCollection services,
        IConfiguration configuration,
        Action<GrafanaProfilingOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var grafanaOptions = configuration.GetSection("Grafana").Get<GrafanaOptions>()
            ?? throw new InvalidOperationException("Missing required configuration section 'Grafana'.");

        if (string.IsNullOrWhiteSpace(grafanaOptions.PyroscopeUrl))
            throw new InvalidOperationException("Missing required configuration value 'Grafana:PyroscopeUrl'.");

        if (Environment.GetEnvironmentVariable("CORECLR_ENABLE_PROFILING") != "1")
            Log.Warning("CORECLR_ENABLE_PROFILING is not set to 1 — " +
                        "native CLR profiler will not collect samples. " +
                        "Add it to launchSettings.json or deployment environment variables.");

        var options = new GrafanaProfilingOptions(
            grafanaOptions.PyroscopeUrl,
            grafanaOptions.ServiceName,
            grafanaOptions.PyroscopeBasicAuthUser);

        configure?.Invoke(options);

        Environment.SetEnvironmentVariable("PYROSCOPE_APPLICATION_NAME", options.ApplicationName);
        Environment.SetEnvironmentVariable("PYROSCOPE_SERVER_ADDRESS", options.PyroscopeUrl);
        Environment.SetEnvironmentVariable("PYROSCOPE_PROFILING_ALLOCATION_ENABLED", options.AllocationProfilingEnabled ? "true" : "false");
        Environment.SetEnvironmentVariable("PYROSCOPE_PROFILING_EXCEPTION_ENABLED", options.ExceptionProfilingEnabled ? "true" : "false");
        Environment.SetEnvironmentVariable("PYROSCOPE_PROFILING_LOCK_ENABLED", options.LockProfilingEnabled ? "true" : "false");

        if (!string.IsNullOrWhiteSpace(options.BasicAuthUser))
            Environment.SetEnvironmentVariable("PYROSCOPE_BASIC_AUTH_USER", options.BasicAuthUser);

        Log.Information("Configuring Pyroscope profiling for {service} → {url}",
            options.ApplicationName, options.PyroscopeUrl);

        var profiler = Pyroscope.Profiler.Instance;
        profiler.SetCPUTrackingEnabled(options.AllocationProfilingEnabled);
        profiler.SetAllocationTrackingEnabled(options.AllocationProfilingEnabled);
        profiler.SetExceptionTrackingEnabled(options.ExceptionProfilingEnabled);
        profiler.SetContentionTrackingEnabled(options.LockProfilingEnabled);

        /*services.AddOpenTelemetry()
            .WithTracing(b =>
            {
                var processor = new Pyroscope.OpenTelemetry.PyroscopeSpanProcessor();
                b.AddConsoleExporter();
                b.AddOtlpExporter();
                b.AddAspNetCoreInstrumentation();
                b.AddProcessor(processor);
            });*/
    }
}
