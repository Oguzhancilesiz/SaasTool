using System.Threading.RateLimiting;

namespace SaasTool.API.Infrastructure.Extensions
{
    public static class RateLimitExtensions
    {
        public static IServiceCollection AddBasicRateLimit(this IServiceCollection s)
        {
            s.AddRateLimiter(o =>
            {
                o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                {
                    var key = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetTokenBucketLimiter(key, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 60,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(60),
                        TokensPerPeriod = 60,
                        AutoReplenishment = true
                    });
                });
                o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });
            return s;
        }
    }

}
