using SaasTool.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class SubscriptionItem : BaseEntity
    {
        public Guid SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = null!;

        public Guid? FeatureId { get; set; }
        public Feature? Feature { get; set; }

        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public Currency Currency { get; set; } = Currency.TRY;
    }

}
