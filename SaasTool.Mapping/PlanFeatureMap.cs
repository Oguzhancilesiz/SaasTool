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
    public class PlanFeatureMap : BaseMap<PlanFeature>
    {
        public override void Configure(EntityTypeBuilder<PlanFeature> b)
        {
            base.Configure(b);
            b.ToTable("PlanFeatures");
            b.HasIndex(x => new { x.PlanId, x.FeatureId }).IsUnique();
            b.HasOne(x => x.Plan).WithMany(x => x.PlanFeatures).HasForeignKey(x => x.PlanId);
            b.HasOne(x => x.Feature).WithMany(x => x.PlanFeatures).HasForeignKey(x => x.FeatureId);
        }
    }

}
