using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaasTool.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Mapping
{
    public class ApiKeyMap : BaseMap<ApiKey>
    {
        public override void Configure(EntityTypeBuilder<ApiKey> b)
        {
            base.Configure(b);
            b.ToTable("ApiKeys");
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.KeyHash).HasMaxLength(200).IsRequired();
            b.HasIndex(x => new { x.OrganizationId, x.AppId, x.KeyHash }).IsUnique();
            b.HasOne(x => x.Organization).WithMany(x => x.ApiKeys).HasForeignKey(x => x.OrganizationId);
            b.HasOne(x => x.App).WithMany().HasForeignKey(x => x.AppId);
        }
    }

}
