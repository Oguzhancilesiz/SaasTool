using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.Entity;
using SaasTool.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DAL
{
    public class BaseContext : IdentityDbContext<AppUser, AppRole, Guid>, IEFContext
    {
        public BaseContext(DbContextOptions options) : base(options)
        {

        }
        public override DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        async Task<int> IEFContext.SaveChangesAsync(CancellationToken cancellationToken)
        {
            string id = Guid.NewGuid().ToString().Replace("-", "") + DateTime.Now.ToBinary();//Log veritabanları için genelde kullanılan işlemID(proccessId) kolonuna eklenecek veriyi temsil eder.

            //HttpContext.User.Identity.Name
            //HttpContext.User.Claims.GetClaims()

            DateTime now = DateTime.Now;

            foreach (var entry in ChangeTracker.Entries<IEntity>())
            {
                if (entry.State == EntityState.Added | entry.State == EntityState.Modified | entry.State == EntityState.Deleted)
                {
                    entry.Entity.ModifiedDate = now;
                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.CreatedDate = now;
                        entry.Entity.Status = Core.Enums.Status.Active;
                    }
                }
            }

            int rowCount = 0;

            try
            {
                rowCount = await base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

            return rowCount;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Mapping İşlemleri
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseMap<IEntity>).Assembly);


            base.OnModelCreating(modelBuilder);
        }
    }
}
