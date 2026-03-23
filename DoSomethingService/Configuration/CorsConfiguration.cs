namespace DoSomethingService.Configuration;

public static class CorsConfiguration
{
    public static void SetupCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
                {
                    var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
                    policy.WithOrigins(origins)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                }
            );
        });
    }
}