using DoSomethingService.Configuration;
using DoSomethingService.Configuration.Swagger;
using DoSomethingService.Configuration.Telemetry;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace DoSomethingService;

public class Startup
{
    private readonly IHostEnvironment _environment;

    public Startup(IConfiguration configuration, IHostEnvironment environment)
    {
        Configuration = configuration;
        _environment = environment;
    }
    private const string OutputTemplate =
        "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level:u3}] [{ThreadId}] {Message}{NewLine}{Exception}";
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: OutputTemplate)
            .WriteTo.AddGrafanaLogging(Configuration)
            .CreateLogger();

        services.AddSingleton(Log.Logger);
        services.AddGrafanaTelemetry(Configuration);
        //services.AddGrafanaProfiling(Configuration);
        services.SetupCors(Configuration);

        // Now set up a health check publisher
        services.AddSingleton<IHealthCheckPublisher, HealthCheckPublisher>();
        services.Configure<HealthCheckPublisherOptions>(options =>
        {
            options.Delay = TimeSpan.FromSeconds(2);
            options.Period = TimeSpan.FromSeconds(5);
            options.Timeout = TimeSpan.FromSeconds(5);
            options.Predicate = _ => true;
        });

        services.AddControllers();
        services.AddMvc(c => { c.Conventions.Add(new ApiExplorerGroupPerVersionConvention()); });
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());
        services.SetupSwaggerGen(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

        app.SetupSwaggerUI(Configuration);
        app.UseRouting();
        app.UseCors("CorsPolicy");

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
            endpoints.MapHealthChecks("/ready");
            endpoints.MapHealthChecks("/liveness");
        });

    }

    public class ApiExplorerGroupPerVersionConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var controllerNamespace = controller.ControllerType.Namespace;
            var apiVersion = controllerNamespace.Split('.').Last().ToLower();
            controller.ApiExplorer.GroupName = apiVersion;
        }
    }
}