using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

namespace SaasTool.API.Infrastructure.Extensions
{
    public static class ApiExtensions
    {
        public static IServiceCollection AddApiVersioningAndSwagger(this IServiceCollection s)
        {
            // 1) API Versioning + Explorer (chained)
            var ver = s.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
                o.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            ver.AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });

            // 2) Sadece base servisler (Swagger EKLEMEYİZ; Program.cs’te zaten var)
            s.AddEndpointsApiExplorer();
            s.AddOutputCache(o => o.AddBasePolicy(b => b.Expire(TimeSpan.FromSeconds(5))));
            s.AddHealthChecks();

            return s;
        }
    }
}
