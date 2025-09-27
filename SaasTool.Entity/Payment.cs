using SaasTool.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class Payment : BaseEntity
    {
        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public PaymentProvider Provider { get; set; } = PaymentProvider.Manual;
        public string? ProviderPaymentId { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; } = Currency.TRY;
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    }

}
