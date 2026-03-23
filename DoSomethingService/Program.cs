using System.Diagnostics;
using DoSomethingService;
using DoSomethingService.Configuration.Telemetry;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;
_ = new OtelTraceListener();

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
    .Build()
    .Run();