using System.Diagnostics;
using DoSomethingService;
using DoSomethingService.Configuration.Telemetry;


Activity.DefaultIdFormat = ActivityIdFormat.W3C;
var otelListener = new OtelTraceListener();

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>(); 
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
    })
    .Build()
    .Run();