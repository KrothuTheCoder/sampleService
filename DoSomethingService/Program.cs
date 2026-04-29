using System.Diagnostics;
using DoSomethingService;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;

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