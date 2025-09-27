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
    public class SubscriptionItemMap : BaseMap<SubscriptionItem>
    {
        public override void Configure(EntityTypeBuilder<SubscriptionItem> b)
        {
            base.Configure(b);
            b.ToTable("SubscriptionItems");
            b.HasOne(x => x.Subscription).WithMany(x => x.Items).HasForeignKey(x => x.SubscriptionId);
            b.HasOne(x => x.Feature).WithMany().HasForeignKey(x => x.FeatureId);
        }
    }

}
