using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using SaasTool.Core.Abstracts;
using SaasTool.Entity;
using System.Linq.Expressions;

namespace SaasTool.DAL
{
    public class BaseContext : IdentityDbContext<AppUser, AppRole, Guid>, IEFContext
    {
        public BaseContext(DbContextOptions options) : base(options) { }

        public override DbSet<TEntity> Set<TEntity>() where TEntity : class => base.Set<TEntity>();

        async Task<int> IEFContext.SaveChangesAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<IEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedDate = now;
                    entry.Entity.Status = Core.Enums.Status.Active;
                }
                if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                    entry.Entity.ModifiedDate = now;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SaasTool.Mapping.BaseMap<>).Assembly);

            // Global soft-delete filter: Status != Deleted
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(IEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var param = Expression.Parameter(entityType.ClrType, "e");
                    var statusProp = Expression.Property(param, nameof(IEntity.Status));
                    var deletedConst = Expression.Constant(SaasTool.Core.Enums.Status.Deleted);
                    var body = Expression.NotEqual(statusProp, deletedConst);
                    var lambda = Expression.Lambda(body, param);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }
    }
}
