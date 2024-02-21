using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerUI;



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

builder.Services.AddControllers();
builder.Services.AddApiVersioning();

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });



var provider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

builder.Services.AddSwaggerGen(
    options =>
    {
       
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, new OpenApiInfo
                    {
                        Title = $"Your API {description.ApiVersion}",
                        Version = description.ApiVersion.ToString()
                    });
                }
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    app.UseSwaggerUI(options =>
    {
        // Display Swagger UI for each discovered API version
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
        options.DocExpansion(DocExpansion.None);
    });
});

app.UseHttpsRedirection();
app.UseCors(myAllowSpecificOrigins);

app.MapControllers().WithOpenApi();

app.Run();