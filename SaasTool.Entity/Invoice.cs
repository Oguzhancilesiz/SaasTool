using SaasTool.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    // Invoice.cs
    public class Invoice : BaseEntity
    {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public string InvoiceNumber { get; set; } = null!;

        // Eskiden: public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        public InvoiceStatus InvoiceState { get; set; } = InvoiceStatus.Draft;

        public Currency Currency { get; set; } = Currency.TRY;

        public decimal Subtotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime? PaidAt { get; set; }

        public PaymentProvider Provider { get; set; } = PaymentProvider.Manual;
        public string? ProviderInvoiceId { get; set; }
        public string? ProviderPaymentIntentId { get; set; }

        public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

}
