using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace SaasTool.API.Infrastructure.Middleware
{
    public class ProblemDetailsMiddleware : IMiddleware
    {
        private readonly ILogger<ProblemDetailsMiddleware> _logger;
        public const string CorrelationHeader = "X-Request-Id";

        public ProblemDetailsMiddleware(ILogger<ProblemDetailsMiddleware> logger) { _logger = logger; }

        public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
        {
            // Correlation Id ata/taşı
            if (!ctx.Request.Headers.TryGetValue(CorrelationHeader, out var corr) || string.IsNullOrWhiteSpace(corr))
                ctx.Response.Headers[CorrelationHeader] = Guid.NewGuid().ToString();
            else
                ctx.Response.Headers[CorrelationHeader] = corr.ToString();

            try
            {
                await next(ctx);
            }
            catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
            {
                // 499 - Client Closed Request ( resmi enum’da yok )
                ctx.Response.StatusCode = 499;
                return;
            }

            catch (Exception ex)
            {
                var traceId = Activity.Current?.Id ?? ctx.TraceIdentifier;
                var title = "Beklenmeyen bir hata oluştu.";
                var pd = new ProblemDetails
                {
                    Title = title,
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = ex.Message,
                    Instance = ctx.Request.Path,
                    Extensions =
                {
                    ["traceId"] = traceId,
                    ["requestId"] = ctx.Response.Headers[CorrelationHeader].ToString()
                }
                };

                _logger.LogError(ex, "Unhandled exception. {TraceId}", traceId);

                ctx.Response.ContentType = "application/problem+json";
                ctx.Response.StatusCode = pd.Status.Value;
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(pd));
            }
        }
    }

}
