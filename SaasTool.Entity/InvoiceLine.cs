using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class InvoiceLine : BaseEntity
    {
        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public string Description { get; set; } = null!;
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }

        public Guid? FeatureId { get; set; }
        public Guid? SubscriptionId { get; set; }
        public Subscription? Subscription { get; set; }
    }

}
