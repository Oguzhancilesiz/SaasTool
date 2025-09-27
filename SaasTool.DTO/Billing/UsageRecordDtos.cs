using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Billing
{
    public record UsageRecordDto : AuditedDto
    {
        public Guid SubscriptionId { get; init; }
        public Guid FeatureId { get; init; }
        public int PeriodUnit { get; init; } // enum int
        public DateTime PeriodStart { get; init; }
        public DateTime PeriodEnd { get; init; }
        public decimal UsedValue { get; init; }
    }

    public record UsageRecordCreateDto
    {
        public Guid SubscriptionId { get; init; }
        public Guid FeatureId { get; init; }
        public int PeriodUnit { get; init; }
        public DateTime PeriodStart { get; init; }
        public DateTime PeriodEnd { get; init; }
        public decimal UsedValue { get; init; }
    }

}
