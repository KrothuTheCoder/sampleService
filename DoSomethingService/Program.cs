using Microsoft.OpenApi.Models;

var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
var apiVersions = new Dictionary<string, string>();
apiVersions.Add("v1", "SomethingV1");

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(
    options =>
    {
        // build a swagger endpoint for each discovered API version
        foreach (var description in apiVersions)
        {
            var url = $"/swagger/{description.Key}/swagger.json";
            var name = description.Value.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
    });

app.UseHttpsRedirection();
app.UseCors(myAllowSpecificOrigins);
//app.UseAuthorization();

app.MapControllers();

app.Run();