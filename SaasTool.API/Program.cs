using Asp.Versioning.ApiExplorer;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaasTool.API.Infrastructure.Extensions;
using SaasTool.Core.Abstracts;
using SaasTool.Core.Security;
using SaasTool.DAL;
using SaasTool.DAL.Seed;
using SaasTool.DAL.Time;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;
using SaasTool.Service.Concrete;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace SaasTool.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

        builder.Services.AddSingleton<SaasTool.Core.Abstracts.ISystemClock, SystemClock>();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddValidationPipeline();
        builder.Services.AddApiVersioningAndSwagger();   // ← SADECE BU VAR

        builder.Services.AddAuthorization(opts =>
        {
            opts.AddPolicy("Dashboard.View", p => p.RequireRole("SuperAdmin", "OrgAdmin", "Analyst", "Support"));
            opts.AddPolicy("Dashboard.Finance", p => p.RequireRole("SuperAdmin", "OrgAdmin", "Analyst"));
        });

        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        builder.Services.AddDbContext<BaseContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("dbCon"),
                sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

        builder.Services.AddScoped<ITenantContext>(_ => new HttpTenantContext(_.GetRequiredService<IHttpContextAccessor>()));
        builder.Services.AddScoped<ICurrentUser>(_ => new HttpCurrentUser(_.GetRequiredService<IHttpContextAccessor>()));

        builder.Services.AddIdentityCore<AppUser>(opt =>
        {
            opt.User.RequireUniqueEmail = true;
            opt.Password.RequiredLength = 6;
            opt.Password.RequireDigit = false;
            opt.Password.RequireUppercase = false;
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequireLowercase = false;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<BaseContext>()
        .AddSignInManager<SignInManager<AppUser>>();

        builder.Services.AddScoped<IEFContext, BaseContext>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IPlanService, PlanService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IFeatureService, FeatureService>();
        builder.Services.AddScoped<IAppService, AppService>();
        builder.Services.AddScoped<IPlanFeatureService, PlanFeatureService>();
        builder.Services.AddScoped<IOrganizationService, OrganizationService>();
        builder.Services.AddScoped<ICustomerService, CustomerService>();
        builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
        builder.Services.AddScoped<IUsageService, UsageService>();
        builder.Services.AddScoped<IInvoiceService, InvoiceService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<IAppPlanService, AppPlanService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();

        var mapsterCfg = TypeAdapterConfig.GlobalSettings;
        mapsterCfg.Scan(typeof(SaasTool.Service.MapsterMap.MapsterConfig).Assembly);
        builder.Services.AddSingleton(mapsterCfg);
        builder.Services.AddScoped<IMapper, ServiceMapper>();

        builder.Services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        builder.Services.AddProblemDetails(opts =>
        {
            opts.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();
            opts.MapToStatusCode<InvalidOperationException>(StatusCodes.Status400BadRequest);
        });

        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
        builder.Services.AddCors(opt =>
        {
            if (origins.Length > 0)
                opt.AddPolicy("default", p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
            else
                opt.AddPolicy("default", p => p.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true));
        });

        builder.Services.AddBasicRateLimit();

        builder.Services.AddHealthChecks().AddDbContextCheck<BaseContext>("db");
        builder.Services.AddAppHealthChecks(builder.Configuration);

        builder.Services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            var key = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Jwt:Key boş olamaz.");

            o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.FromMinutes(2)
            };
        });
        builder.Services.AddHostedService<SeedHostedService>();


        //builder.Services.AddTransient<ProblemDetailsMiddleware>();
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("auth-policy", httpContext =>
            {
                var userId = httpContext.User?.Identity?.IsAuthenticated == true
                    ? httpContext.User.Identity!.Name ?? httpContext.User.FindFirst("sub")?.Value ?? "auth"
                    : httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon";

                return RateLimitPartition.GetTokenBucketLimiter(userId, key => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = httpContext.User.Identity?.IsAuthenticated == true ? 120 : 30,
                    QueueLimit = 0,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(60),
                    TokensPerPeriod = httpContext.User.Identity?.IsAuthenticated == true ? 120 : 30,
                    AutoReplenishment = true
                });
            });
        });


        var app = builder.Build();

        // Request Id header (opsiyonel ama faydalı)
        app.Use(async (ctx, next) =>
        {
            if (!ctx.Request.Headers.TryGetValue("X-Request-Id", out var v) || string.IsNullOrWhiteSpace(v))
                ctx.Response.Headers["X-Request-Id"] = Guid.NewGuid().ToString();
            else
                ctx.Response.Headers["X-Request-Id"] = v.ToString();
            await next();
        });

        //app.UseMiddleware<SaasTool.API.Infrastructure.Middleware.ProblemDetailsMiddleware>();


        app.MapControllers().RequireRateLimiting("auth-policy");


        ///seed data için
        //using (var scope = app.Services.CreateScope())
        //{
        //    var db = scope.ServiceProvider.GetRequiredService<BaseContext>();
        //    await db.Database.MigrateAsync();                 // migrationları uygula
        //    await SaasMegaSeed.RunAsync(app.Services);        // bol veri topla
        //}

        app.UseSerilogRequestLogging();
        app.UseProblemDetails();

        // Swagger UI
        app.UseSwagger();
        app.UseSwaggerUI(opt =>
        {
            // Versiyonlu dokümanlar için explorer'dan oku
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var desc in provider.ApiVersionDescriptions)
                opt.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", $"SaasTool API {desc.GroupName}");
        });

        app.UseHttpsRedirection();
        app.UseCors("default");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health");
        app.MapHealthChecks("/readyz");

        await app.RunAsync();
    }

    // Stublar (senin kullandıkların)
    public sealed class HttpTenantContext : ITenantContext
    {
        public Guid? Id { get; set; }
        public string? Code { get; set; }

        public HttpTenantContext(IHttpContextAccessor accessor)
        {
            var http = accessor.HttpContext;
            if (http is not null && http.Request.Headers.TryGetValue("X-Tenant-Id", out var v) && Guid.TryParse(v.ToString(), out var g))
                Id = g;
            if (http is not null && http.Request.Headers.TryGetValue("X-Tenant-Code", out var vc))
                Code = vc.ToString();
        }
    }

    public sealed class HttpCurrentUser : ICurrentUser
    {
        public Guid? UserId { get; }
        public string? Email { get; }
        public bool IsAuthenticated { get; }
        private readonly ClaimsPrincipal _user;

        public HttpCurrentUser(IHttpContextAccessor accessor)
        {
            _user = accessor.HttpContext?.User ?? new ClaimsPrincipal();
            IsAuthenticated = _user.Identity?.IsAuthenticated ?? false;
            if (IsAuthenticated)
            {
                var id = _user.FindFirst("sub")?.Value ?? _user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(id, out var g)) UserId = g;
                Email = _user.FindFirst(ClaimTypes.Email)?.Value;
            }
        }

        public bool IsInRole(string role) => _user.IsInRole(role);
    }
}
