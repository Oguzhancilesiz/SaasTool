using SaasTool.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class Coupon : BaseEntity
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DiscountType DiscountType { get; set; }
        public decimal Value { get; set; }
        public Currency? Currency { get; set; } // FixedAmount için zorunlu
        public int? MaxRedemptions { get; set; }
        public DateTime? RedeemBy { get; set; }
        public bool IsActive { get; set; } = true;

        public Guid? AppliesToAppId { get; set; }
        public Guid? AppliesToPlanId { get; set; }
    }

}
