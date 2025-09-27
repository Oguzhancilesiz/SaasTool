using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SaasTool.Core.Abstracts;    // IEFContext, ISystemClock
using SaasTool.Entity;            // AppUser, AppRole, IEntity
using SaasTool.Core.Enums;        // Status

namespace SaasTool.DAL
{
    public class BaseContext : IdentityDbContext<AppUser, AppRole, Guid>, IEFContext
    {
        private readonly ISystemClock? _clock; // DateTimeOffset döner
        public BaseContext(DbContextOptions options) : base(options) { }
        public BaseContext(DbContextOptions options, ISystemClock clock) : base(options) => _clock = clock;

        public override DbSet<TEntity> Set<TEntity>() where TEntity : class => base.Set<TEntity>();

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => SaveChangesCoreAsync(cancellationToken);

        Task<int> IEFContext.SaveChangesAsync(CancellationToken cancellationToken)
            => SaveChangesCoreAsync(cancellationToken);

        private async Task<int> SaveChangesCoreAsync(CancellationToken ct)
        {
            // >>> FARK: DateTimeOffset -> DateTime (UTC) dönüştürüyoruz
            var now = (_clock?.UtcNow ?? DateTimeOffset.UtcNow).UtcDateTime;

            foreach (var entry in ChangeTracker.Entries<IEntity>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.Status = Status.Deleted;
                    entry.Entity.ModifiedDate = now;
                    continue;
                }

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedDate = now;
                    if (entry.Entity.Status == 0)
                        entry.Entity.Status = Status.Active;
                    entry.Entity.ModifiedDate = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedDate = now;
                }
            }

            try
            {
                return await base.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw; // middleware 409’a çeviriyor
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SaasTool.Mapping.BaseMap<>).Assembly);

            // Global soft-delete filter
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clr = entityType.ClrType;
                if (typeof(IEntity).IsAssignableFrom(clr))
                {
                    var param = Expression.Parameter(clr, "e");
                    var statusProp = Expression.Property(param, nameof(IEntity.Status));
                    var deletedConst = Expression.Constant(Status.Deleted);
                    var body = Expression.NotEqual(statusProp, deletedConst);
                    var lambda = Expression.Lambda(body, param);
                    modelBuilder.Entity(clr).HasQueryFilter(lambda);
                }
            }

            // RowVersion varsa concurrency token
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var rowVersionProp = entity.FindProperty("RowVersion");
                if (rowVersionProp is not null && rowVersionProp.ClrType == typeof(byte[]))
                {
                    rowVersionProp.IsConcurrencyToken = true;
                    rowVersionProp.ValueGenerated = ValueGenerated.OnAddOrUpdate;
                }
            }

            // Decimal precision
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var p in entity.GetProperties().Where(p => p.ClrType == typeof(decimal)))
                {
                    p.SetPrecision(18);
                    p.SetScale(2);
                }
            }
        }
    }
}
