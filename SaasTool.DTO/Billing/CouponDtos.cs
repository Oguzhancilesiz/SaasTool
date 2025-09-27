using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Billing
{
    public record CouponDto : AuditedDto
    {
        public string Code { get; init; } = default!;
        public string Name { get; init; } = default!;
        public int DiscountType { get; init; }   // enum int
        public decimal Value { get; init; }
        public int? Currency { get; init; }
        public int? MaxRedemptions { get; init; }
        public DateTime? RedeemBy { get; init; }
        public bool IsActive { get; init; }
        public Guid? AppliesToAppId { get; init; }
        public Guid? AppliesToPlanId { get; init; }
    }

    public record CouponCreateDto
    {
        public string Code { get; init; } = default!;
        public string Name { get; init; } = default!;
        public int DiscountType { get; init; }
        public decimal Value { get; init; }
        public int? Currency { get; init; }
        public int? MaxRedemptions { get; init; }
        public DateTime? RedeemBy { get; init; }
        public bool IsActive { get; init; } = true;
        public Guid? AppliesToAppId { get; init; }
        public Guid? AppliesToPlanId { get; init; }
    }

    public record CouponUpdateDto
    {
        public string Name { get; init; } = default!;
        public bool IsActive { get; init; }
    }

}
