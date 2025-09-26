using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.DAL;
using SaasTool.Entity;

namespace SaasTool.WebUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<BaseContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("dbCon")));

            //Session'ý kullanabilmek için gerekli ayarlarý yapýyoruz.
            builder.Services.AddSession(option =>
            {
                option.IdleTimeout = TimeSpan.FromMinutes(30); //Session 30 dakika sonra sonlanacak.
                option.Cookie.HttpOnly = true; //Cookie'ye javascript eriþimini engelliyoruz.
                option.Cookie.IsEssential = true; //Session'ý kullanabilmek için gerekli.

            });

            builder.Services.AddIdentity<AppUser, AppRole>(option =>
            {
                option.User.RequireUniqueEmail = true;
                option.Password.RequiredLength = 3;
                option.Password.RequireDigit = false;
                option.Password.RequiredUniqueChars = 0;
                option.Password.RequireUppercase = false;
                option.Password.RequireNonAlphanumeric = false;
                option.Password.RequireLowercase = false;
            }).AddEntityFrameworkStores<BaseContext>();

            //IOC -> Razor View Engine Dependency Injection yapmak için hangi interface hangi classla eþleþiyor buradan bilgi alýyor.
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IEFContext, BaseContext>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                       name: "areas",
                       pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                     );
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();


            app.Run();
        }
    }
}
