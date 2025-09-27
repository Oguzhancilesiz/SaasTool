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
    public class AppPlanMap : BaseMap<AppPlan>
    {
        public override void Configure(EntityTypeBuilder<AppPlan> b)
        {
            base.Configure(b);
            b.ToTable("AppPlans");
            b.HasIndex(x => new { x.AppId, x.PlanId }).IsUnique();
            b.HasOne(x => x.App).WithMany(x => x.AppPlans).HasForeignKey(x => x.AppId);
            b.HasOne(x => x.Plan).WithMany(x => x.AppPlans).HasForeignKey(x => x.PlanId);
        }
    }

}
