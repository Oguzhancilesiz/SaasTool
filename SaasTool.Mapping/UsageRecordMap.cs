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
    public class UsageRecordMap : BaseMap<UsageRecord>
    {
        public override void Configure(EntityTypeBuilder<UsageRecord> b)
        {
            base.Configure(b);
            b.ToTable("UsageRecords");
            b.HasIndex(x => new { x.SubscriptionId, x.FeatureId, x.PeriodStart, x.PeriodEnd }).IsUnique();
            b.HasOne(x => x.Subscription).WithMany(x => x.UsageRecords).HasForeignKey(x => x.SubscriptionId);
            b.HasOne(x => x.Feature).WithMany().HasForeignKey(x => x.FeatureId);
        }
    }

}
