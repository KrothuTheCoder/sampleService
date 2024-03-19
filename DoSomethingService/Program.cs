using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using Microsoft.ApplicationInsights.Extensibility;
using System.Net;


var myAllowSpecificOrigins = "myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITelemetryInitializer, UpstreamProxyTraceHeaderTelemetryInitializer>((serviceProvider)=> {
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    // Az App Gateway and Front door trace/correlation headers are defaulted
    return new UpstreamProxyTraceHeaderTelemetryInitializer(httpContextAccessor);
});

builder.Services.AddHttpContextAccessor();

var aiOptions = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions();
aiOptions.EnableAdaptiveSampling = false;
aiOptions.EnableQuickPulseMetricStream = true;
aiOptions.EnableRequestTrackingTelemetryModule = true;
aiOptions.EnableDependencyTrackingTelemetryModule = true;

builder.Services.AddApplicationInsightsTelemetry(aiOptions);

var apiVersions = new Dictionary<string, string>();
//
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000","http://web.hakabo.com","http://api.hakabo.com","https://api2.ipa.sandbox.net")
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
       
});

//
builder.Services.AddControllers();
builder.Services.AddMvc(c=> {
    c.Conventions.Add(new ApiExplorerGroupPerVersionConvention());
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddServer(new OpenApiServer(){
        Url = "https://thedosomethingservice.azurewebsites.net/"
        //Url = "http://localhost:5150/"
    });
     options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API - V1", Version = "v1" ,
        Description = "An ASP.NET Core Web API for managing ToDo items",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }});
     options.SwaggerDoc("v2", new OpenApiInfo { Title = "My API - V2", Version = "v2",
        Description = "An ASP.NET Core Web API for managing ToDo items",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        } });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "My API V2");
    });

    options.DefaultModelExpandDepth(2);
    options.DefaultModelRendering(ModelRendering.Model);
    options.DefaultModelsExpandDepth(-1);
    options.DisplayOperationId();
    options.DisplayRequestDuration();
    options.DocExpansion(DocExpansion.None);
    options.EnableDeepLinking();
    options.EnableFilter();
    options.MaxDisplayedTags(5);
    options.ShowExtensions();
    options.ShowCommonExtensions();
    options.EnableValidator();
    options.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Head);
    options.UseRequestInterceptor("(request) => { return request; }");
    options.UseResponseInterceptor("(response) => { return response; }");
});

app.UseHttpsRedirection();


app.MapControllers();

app.UseCors(myAllowSpecificOrigins);
app.Run();

public class ApiExplorerGroupPerVersionConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var controllerNamespace = controller.ControllerType.Namespace;
        var apiVersion = controllerNamespace.Split('.').Last().ToLower();
        controller.ApiExplorer.GroupName=apiVersion;
    }
}
