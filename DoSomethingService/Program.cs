using Microsoft.OpenApi.Models;
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
options.AddPolicy(name: MyAllowSpecificOrigins,
policy =>
{
policy.WithOrigins("https://bobthebuilder.azure-api.net","https://thedosomethingservice.azurewebsites.net");
});
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => 
{ options.SwaggerDoc("v1", new OpenApiInfo
{
Version = "v1.1",
Title = "The DoSomething API"
});
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
//app.UseAuthorization();

app.MapControllers();

app.Run();
