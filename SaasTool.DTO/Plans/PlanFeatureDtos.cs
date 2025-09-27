using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Plans
{
    public record PlanFeatureDto : AuditedDto
    {
        public Guid PlanId { get; init; }
        public Guid FeatureId { get; init; }
        public int Unit { get; init; }               // FeatureUnit enum int
        public decimal? LimitValue { get; init; }
        public bool OveragesEnabled { get; init; }
        public decimal? OverageUnitPrice { get; init; }
    }

    public record PlanFeatureCreateDto
    {
        public Guid PlanId { get; init; }
        public Guid FeatureId { get; init; }
        public int Unit { get; init; }
        public decimal? LimitValue { get; init; }
        public bool OveragesEnabled { get; init; }
        public decimal? OverageUnitPrice { get; init; }
    }

    public record PlanFeatureUpdateDto
    {
        public int Unit { get; init; }
        public decimal? LimitValue { get; init; }
        public bool OveragesEnabled { get; init; }
        public decimal? OverageUnitPrice { get; init; }
    }

}
