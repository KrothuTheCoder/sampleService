var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen("v1", new OpenApiInfo
{
Version = "v1.1"
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
