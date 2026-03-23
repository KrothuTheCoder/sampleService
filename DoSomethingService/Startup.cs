

using System.Diagnostics;
using DoSomethingService.Telemetry;
using Serilog;

public class Startup
{
    public IConfiguration Configuration { get; }
    private readonly IHostEnvironment _environment;
    
    public Startup(IConfiguration configuration, IHostEnvironment environment)
    {
        Configuration = configuration;
        _environment = environment;
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration)
            .Enrich.FromLogContext()
            .WriteTo.AddGrafanaLogging(Configuration)
            .WriteTo.Console()
            .CreateLogger();
        
        services.AddSingleton(Log.Logger);
        services.AddGrafanaTelemetry(Configuration);
        
    }
}