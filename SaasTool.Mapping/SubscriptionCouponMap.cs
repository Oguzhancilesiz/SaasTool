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
    public class SubscriptionCouponMap : BaseMap<SubscriptionCoupon>
    {
        public override void Configure(EntityTypeBuilder<SubscriptionCoupon> b)
        {
            base.Configure(b);
            b.ToTable("SubscriptionCoupons");
            b.HasIndex(x => new { x.SubscriptionId, x.CouponId }).IsUnique();
            b.HasOne(x => x.Subscription).WithMany(x => x.Coupons).HasForeignKey(x => x.SubscriptionId);
            b.HasOne(x => x.Coupon).WithMany().HasForeignKey(x => x.CouponId);
        }
    }

}
