using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace SaasTool.API.Infrastructure.Middleware
{
    public sealed class CorrelationIdMiddleware
    {
        public const string Header = "X-Correlation-ID";
        private readonly RequestDelegate _next;
        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx)
        {
            var id = ctx.Request.Headers.TryGetValue(Header, out StringValues v) && !StringValues.IsNullOrEmpty(v)
                ? v.ToString()
                : Guid.NewGuid().ToString("N");

            ctx.Response.Headers[Header] = id;
            using (LogContext.PushProperty("CorrelationId", id))
                await _next(ctx);
        }
    }
}
