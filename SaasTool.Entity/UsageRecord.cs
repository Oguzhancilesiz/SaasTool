using SaasTool.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class UsageRecord : BaseEntity
    {
        public Guid SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = null!;

        public Guid FeatureId { get; set; }
        public Feature Feature { get; set; } = null!;

        public PeriodUnit PeriodUnit { get; set; } = PeriodUnit.Day; // günlük/haftalık/aylık özet
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal UsedValue { get; set; }
    }

}
