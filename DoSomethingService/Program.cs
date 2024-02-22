using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApplicationModels;


var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

var apiVersions = new Dictionary<string, string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://bobthebuilder.azure-api.net",
                "https://thedosomethingservice.azurewebsites.net");
        });
});

// builder.Services.AddControllers();
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new GroupingByNamespaceConvention());
});

builder.Services.AddApiVersioning();

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new HeaderApiVersionReader("api-version");
    })
    .AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddSwaggerGen(options =>
{
     options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API - V1", Version = "v1" });

    // var provider = builder.GetRequiredService<IApiVersionDescriptionProvider>();

    // foreach (var description in provider.ApiVersionDescriptions)
    // {
    //     options.SwaggerDoc(description.GroupName, new OpenApiInfo
    //     {
    //         Title = $"Your API {description.ApiVersion}",
    //         Version = description.ApiVersion.ToString()
    //     });
    // }
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");

        // Display Swagger UI for each discovered API version
        // foreach (var description in provider.ApiVersionDescriptions)
        // {
        //     options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        // }
        // options.DocExpansion(DocExpansion.None);
    });
});

app.UseHttpsRedirection();
app.UseCors(myAllowSpecificOrigins);

app.MapControllers().WithOpenApi();

app.Run();

public class GroupingByNamespaceConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var controllerNamespace = controller.ControllerType.Namespace;
        var apiVersion = controllerNamespace.Split(".").Last().ToLower();
        if (!apiVersion.StartsWith("v")) { apiVersion = "v1"; }
        controller.ApiExplorer.GroupName = apiVersion;
    }
}