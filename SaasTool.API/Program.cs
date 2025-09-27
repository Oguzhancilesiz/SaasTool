using Asp.Versioning;
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
// DİKKAT: Microsoft.Extensions.Internal using'ini KALDIRDIK (çakışma yaratıyordu)
using Microsoft.OpenApi.Models;
using SaasTool.API.Infrastructure.Extensions;
using SaasTool.Core.Abstracts;
using SaasTool.Core.Security;
using SaasTool.DAL;
using SaasTool.DAL.Time;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;
using SaasTool.Service.Concrete;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace SaasTool.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Logging: Serilog
            builder.Host.UseSerilog((ctx, lc) =>
                lc.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

            // ── Core singletons
            builder.Services.AddSingleton<SaasTool.Core.Abstracts.ISystemClock, SystemClock>(); // ÇAKIŞMASIN
            builder.Services.AddHttpContextAccessor();

            // ── Validation + ProblemDetails + API versioning + Swagger + OutputCache + Health
            builder.Services.AddValidationPipeline();               // bizim extension: FluentValidation + ModelState->ProblemDetails
            builder.Services.AddApiVersioningAndSwagger();          // bizim extension: v1 + explorer + swagger + output cache + health base

            // ── Authorization + Permission policies
            builder.Services.AddAuthorization();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            // ── DbContext (retry-on-failure ile)
            builder.Services.AddDbContext<BaseContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("dbCon"),
                    sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

            // ── Tenant & CurrentUser (stub)
            builder.Services.AddScoped<ITenantContext>(_ => new HttpTenantContext(_.GetRequiredService<IHttpContextAccessor>()));
            builder.Services.AddScoped<ICurrentUser>(_ => new HttpCurrentUser(_.GetRequiredService<IHttpContextAccessor>()));

            // ── Identity Core
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

            // ── IoC
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

            // ── Mapster
            var mapsterCfg = TypeAdapterConfig.GlobalSettings;
            mapsterCfg.Scan(typeof(SaasTool.Service.MapsterMap.MapsterConfig).Assembly);
            builder.Services.AddSingleton(mapsterCfg);
            builder.Services.AddScoped<IMapper, ServiceMapper>();

            // ── Controllers + JSON
            builder.Services.AddControllers()
                .AddJsonOptions(o =>
                    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

            // ── FluentValidation
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            // ── Hellang ProblemDetails (opsiyonel ama sende var)
            builder.Services.AddProblemDetails(opts =>
            {
                opts.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();
                opts.MapToStatusCode<InvalidOperationException>(StatusCodes.Status400BadRequest);
            });

            // ── Swagger’da Bearer şeması
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SaasTool API", Version = "v1" });

                var jwtScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Bearer {token}"
                };
                c.AddSecurityDefinition("Bearer", jwtScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    [jwtScheme] = Array.Empty<string>()
                });
            });

            // ── CORS
            var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
            builder.Services.AddCors(opt =>
            {
                if (origins.Length > 0)
                {
                    opt.AddPolicy("default", p => p
                        .WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
                }
                else
                {
                    // Dev için geniş: cred yok
                    opt.AddPolicy("default", p => p
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(_ => true));
                }
            });

            // ── Rate limit
            builder.Services.AddBasicRateLimit();

            // ── HealthChecks (ek)
            builder.Services.AddHealthChecks().AddDbContextCheck<BaseContext>("db");
            builder.Services.AddAppHealthChecks(builder.Configuration);

            // ── JWT Auth
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                var key = builder.Configuration["Jwt:Key"];
                if (string.IsNullOrWhiteSpace(key))
                    throw new InvalidOperationException("Jwt:Key boş olamaz.");

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });

            var app = builder.Build(); // <<< app'ten önce hiçbir app.Use... çağrısı yok

            // ── Pipeline
            app.UseSerilogRequestLogging();
            app.UseProblemDetails();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("default");
            app.UseRateLimiter();          // <<< BURAYA TAŞINDI
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");
            app.MapHealthChecks("/readyz"); // <<< BURAYA TAŞINDI

            await app.RunAsync();
        }

        // --- Basit HTTP-tabanlı implementasyonlar (sende gerçekleriyle değiştirebilirsin) ---
        public sealed class HttpTenantContext : ITenantContext
        {
            public Guid? Id { get; set; }     // interface set istiyorsa uyumlu
            public string? Code { get; set; } // interface set istiyorsa uyumlu

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
}
