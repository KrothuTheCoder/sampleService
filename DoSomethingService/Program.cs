using Microsoft.OpenApi.Models;

var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
var apiVersions = new Dictionary<string, string>();
apiVersions.Add("v1", "SomethingV1");
apiVersions.Add("v2", "SomethingV2");
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
/*options =>
{
    /*options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "The DoSomething API"
    });
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "The DoSomething API"
    });#1#
    //options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});*/

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

/*options =>*/
    /*{
        options.SwaggerEndpoint($"/v1/Something/swagger/swagger.json", "SomethingV1");
        options.SwaggerEndpoint($"/swagger/v2/Something/swagger.json", "SomethingV2");
    }*/
    /*options =>
    {
        // build a swagger endpoint for each discovered API version
        foreach (var description in apiVersions)
        {
            var name = description.Value.ToUpperInvariant();
            var url = $"/swagger/{description.Key}/Something/swagger.json";
            options.SwaggerEndpoint(url, name);
        }
    }*/

app.UseHttpsRedirection();
app.UseCors(myAllowSpecificOrigins);
//app.UseAuthorization();

app.MapControllers().WithOpenApi();

app.Run();