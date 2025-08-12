using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;

using ISPL.NetCoreFramework.Helpers;

using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ISPL.NetCoreFramework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
        {
            AppSettingsHelper.Init(config);

            services.AddHttpContextAccessor();
            services.AddAuthenticationJwtBearer(s => s.SigningKey = AppSettingsHelper.Get("JwtSetting:Key"))
                    .AddAuthorization()
                    .AddFastEndpoints()
                    .AddOData(o => o.SetCaseInsensitive(true))
                    .AddAntiforgery()
                    .AddResponseCaching();

            services.SwaggerDocument(o =>
            {
                o.DocumentSettings = s =>
                {
                    s.Title = AppSettingsHelper.Get("Swagger:Title");
                    s.Version = "1.0";
                    s.DocumentName = "v1";
                };
            });
            services.AddCors(options =>
            {
                options.AddPolicy(
                 name: "CorsPolicy",
                 builder => builder.AllowAnyOrigin()
                                   .AllowAnyHeader()
                                   .AllowAnyMethod()
                 );
            });
            return services;
        }
    }
}
