using SaasTool.Core.Enums;
using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Plans
{
    public record PlanDto : AuditedDto
    {
        public string Code { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
        public Currency Currency { get; init; }         // Currency enum dışarı int
        public decimal Price { get; init; }
        public BillingPeriod BillingPeriod { get; init; }     // BillingPeriod enum int
        public bool IsPublic { get; init; }
        public int TrialDays { get; init; }
    }

    public record PlanCreateDto
    {
        public string Code { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
        public Currency Currency { get; init; }
        public decimal Price { get; init; }
        public BillingPeriod BillingPeriod { get; init; }
        public bool IsPublic { get; init; } = true;
        public int TrialDays { get; init; } = 0;
    }

    public record PlanUpdateDto
    {
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public bool IsPublic { get; init; }
        public int TrialDays { get; init; }
    }


}
