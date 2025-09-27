using FluentValidation;                 // ← doğru paket
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SaasTool.API.Infrastructure.Middleware
{
    public sealed class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionMiddleware> _log;
        public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> log)
        { _next = next; _log = log; }

        public async Task Invoke(HttpContext ctx)
        {
            try { await _next(ctx); }
            catch (ValidationException ex)
            {
                var details = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                await Write(ctx, StatusCodes.Status400BadRequest, "ValidationFailed", details);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _log.LogWarning(ex, "Concurrency conflict");
                await Write(ctx, StatusCodes.Status409Conflict, "ConcurrencyConflict", "Kaynak başka biri tarafından güncellendi.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unhandled");
                await Write(ctx, StatusCodes.Status500InternalServerError, "ServerError", "Beklenmeyen bir hata oluştu.");
            }
        }

        private static async Task Write(HttpContext ctx, int status, string code, object? details = null)
        {
            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode = status;
            var p = new ProblemDetails
            {
                Status = status,
                Title = code,
                Type = $"urn:error:{code}"
            };
            if (details != null) p.Extensions["details"] = details;
            p.Extensions["traceId"] = ctx.TraceIdentifier;
            await ctx.Response.WriteAsJsonAsync(p);
        }
    }
}
