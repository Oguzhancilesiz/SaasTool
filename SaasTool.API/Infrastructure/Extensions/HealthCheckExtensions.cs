using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SaasTool.API.Infrastructure.Extensions
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddAppHealthChecks(this IServiceCollection s, IConfiguration cfg)
        {
            s.AddHealthChecks()
             .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready" });
            // .AddNpgSql(cfg.GetConnectionString("Default"))  // Postgres kullanıyorsan aç
            // .AddRedis(cfg.GetConnectionString("Redis"))
            return s;
        }
    }

}
