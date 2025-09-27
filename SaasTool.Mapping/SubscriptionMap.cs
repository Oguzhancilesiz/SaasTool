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
    // SubscriptionMap.cs
    public class SubscriptionMap : BaseMap<Subscription>
    {
        public override void Configure(EntityTypeBuilder<Subscription> b)
        {
            base.Configure(b);
            b.ToTable("Subscriptions");

            // Eskiden Status index
            b.HasIndex(x => new { x.OrganizationId, x.AppId, x.SubscriptionState });

            b.HasOne(x => x.Organization).WithMany(x => x.Subscriptions).HasForeignKey(x => x.OrganizationId);
            b.HasOne(x => x.App).WithMany().HasForeignKey(x => x.AppId);
            b.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId);
            b.HasOne(x => x.Customer).WithMany(x => x.Subscriptions).HasForeignKey(x => x.CustomerId);
        }
    }

}
