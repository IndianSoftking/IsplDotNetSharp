using FastEndpoints;
using FastEndpoints.Swagger;

using ISPL.NetCoreFramework.Middlewares;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace ISPL.NetCoreFramework
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApps(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCors("CorsPolicy");
            app.UseAntiforgeryFE();
            app.UseResponseCaching();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            app.UseFastEndpoints(c =>
            {
                c.Endpoints.RoutePrefix = "api";
            });
            app.UseMiddleware<CamelCaseMiddleware>();

            if(app.ApplicationServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
            {
                app.UseSwaggerGen();
            }
            return app;
        }
    }
}
