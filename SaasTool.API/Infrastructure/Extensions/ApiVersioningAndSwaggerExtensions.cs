using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace SaasTool.API.Infrastructure.Extensions;

public static class ApiVersioningAndSwaggerExtensions
{
    public static IServiceCollection AddApiVersioningAndSwagger(this IServiceCollection services)
    {
        // 8.x doğru kullanım: AddApiVersioning().AddApiExplorer(...)
        services
            .AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = new UrlSegmentApiVersionReader(); // /api/v{version}/...
            })
            .AddApiExplorer(opt =>
            {
                opt.GroupNameFormat = "'v'VVV"; // v1, v1.0
                opt.SubstituteApiVersionInUrl = true;
            });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SaasTool API", Version = "v1" });

            var jwtScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Bearer {token}",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            c.AddSecurityDefinition("Bearer", jwtScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtScheme, Array.Empty<string>() }
            });
        });

        return services;
    }
}
