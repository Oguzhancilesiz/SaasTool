using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hellang.Middleware.ProblemDetails;
using Serilog;
using SaasTool.DAL;
using SaasTool.Entity;
using SaasTool.Core.Abstracts;
using Mapster;
using MapsterMapper;

namespace SaasTool.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Serilog
            builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

            // DbContext (retry-on-failure ile)
            builder.Services.AddDbContext<BaseContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("dbCon"),
                    sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

            // Identity
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

            // Auth (JWT ayarlarını sonra ekleyeceğiz)
            builder.Services.AddAuthentication().AddJwtBearer();

            // IoC
            builder.Services.AddScoped<IEFContext, BaseContext>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Mapster (Service projesindeki MapsterConfig’i tara)
            var mapsterCfg = TypeAdapterConfig.GlobalSettings;
            mapsterCfg.Scan(typeof(SaasTool.Service.MapsterMap.MapsterConfig).Assembly);
            builder.Services.AddSingleton(mapsterCfg);
            builder.Services.AddScoped<IMapper, ServiceMapper>();

            // Controllers + ProblemDetails + Swagger
            builder.Services.AddControllers();
            builder.Services.AddProblemDetails(opts =>
            {
                opts.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();
                opts.MapToStatusCode<InvalidOperationException>(StatusCodes.Status400BadRequest);
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // CORS (geliştirmede serbest, prod’da whitelist kullan)
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
                    // Dev için: dikkat, cred ile geniş origin kullanma
                    opt.AddPolicy("default", p => p
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(_ => true));
                }
            });

            // HealthChecks
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<BaseContext>("db");

            var app = builder.Build();

            // Pipeline
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

            // Migrate (dev kolaylığı)
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BaseContext>();
                await db.Database.MigrateAsync();
            }

            await app.RunAsync();
        }
    }
}
