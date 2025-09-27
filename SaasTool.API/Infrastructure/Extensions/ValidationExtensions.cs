using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace SaasTool.API.Infrastructure.Extensions
{
    public static class ValidationExtensions
    {
        public static IServiceCollection AddValidationPipeline(this IServiceCollection s, Assembly? scanAssembly = null)
        {
            s.AddProblemDetails();
            s.Configure<ApiBehaviorOptions>(o =>
            {
                o.InvalidModelStateResponseFactory = ctx =>
                {
                    var errors = ctx.ModelState
                        .Where(kv => kv.Value?.Errors.Count > 0)
                        .ToDictionary(k => k.Key, v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                    var p = new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "ValidationFailed",
                        Type = "urn:error:ValidationFailed"
                    };
                    p.Extensions["details"] = errors;
                    return new BadRequestObjectResult(p) { ContentTypes = { "application/problem+json" } };
                };
            });

            if (scanAssembly is not null)
                s.AddValidatorsFromAssembly(scanAssembly);
            else
                s.AddValidatorsFromAssembly(AppDomain.CurrentDomain.Load("SaasTool.DTO"));

            return s;
        }
    }

}
