using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SaasTool.Core.Abstracts;
using SaasTool.DAL;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;
using SaasTool.Service.Concrete;
using Serilog;
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

            // ── DbContext (retry-on-failure ile)
            builder.Services.AddDbContext<BaseContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("dbCon"),
                    sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

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

            // ── ProblemDetails (Hellang)
            builder.Services.AddProblemDetails(opts =>
            {
                opts.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();
                opts.MapToStatusCode<InvalidOperationException>(StatusCodes.Status400BadRequest);
            });

            // ── API Versioning
            builder.Services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
            }).AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });

            // ── Swagger
            builder.Services.AddEndpointsApiExplorer();
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

            // ── HealthChecks
            builder.Services.AddHealthChecks().AddDbContextCheck<BaseContext>("db");

            // ── JWT Auth (tek kayıt, ayrıntılı)
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

            var app = builder.Build();

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
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            // ── App start
            await app.RunAsync();
        }
    }
}
