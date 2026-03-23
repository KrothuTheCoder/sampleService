using Swashbuckle.AspNetCore.SwaggerUI;

namespace DoSomethingService.Configuration.Swagger;

public static class SwaggerUIConfiguration
{
    public static void SetupSwaggerUI(this IApplicationBuilder app, IConfiguration configuration)
    {
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
    }
}