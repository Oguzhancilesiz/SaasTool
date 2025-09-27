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
    public class FeatureMap : BaseMap<Feature>
    {
        public override void Configure(EntityTypeBuilder<Feature> b)
        {
            base.Configure(b);
            b.ToTable("Features");
            b.Property(x => x.Code).HasMaxLength(120).IsRequired();
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.HasIndex(x => new { x.AppId, x.Code }).IsUnique();
            b.HasOne(x => x.App).WithMany(x => x.Features).HasForeignKey(x => x.AppId);
        }
    }

}
