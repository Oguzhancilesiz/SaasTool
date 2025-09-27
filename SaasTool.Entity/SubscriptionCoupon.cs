using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class SubscriptionCoupon : BaseEntity
    {
        public Guid SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = null!;

        public Guid CouponId { get; set; }
        public Coupon Coupon { get; set; } = null!;

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }

}
