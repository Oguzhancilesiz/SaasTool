using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Billing
{
    public record InvoiceLineDto : AuditedDto
    {
        public Guid InvoiceId { get; init; }
        public string Description { get; init; } = default!;
        public decimal Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal LineTotal { get; init; }
        public Guid? FeatureId { get; init; }
        public Guid? SubscriptionId { get; init; }
    }

    public record InvoiceDto : AuditedDto
    {
        public Guid OrganizationId { get; init; }
        public Guid? CustomerId { get; init; }
        public string InvoiceNumber { get; init; } = default!;
        public int InvoiceState { get; init; }   // enum int
        public int Currency { get; init; }
        public decimal Subtotal { get; init; }
        public decimal TaxTotal { get; init; }
        public decimal GrandTotal { get; init; }
        public DateTime? DueDate { get; init; }
        public DateTime? PaidAt { get; init; }
        public int Provider { get; init; }
        public string? ProviderInvoiceId { get; init; }
        public string? ProviderPaymentIntentId { get; init; }

        public IReadOnlyList<InvoiceLineDto> Lines { get; init; } = Array.Empty<InvoiceLineDto>();
    }

    public record InvoiceCreateDto
    {
        public Guid OrganizationId { get; init; }
        public Guid? CustomerId { get; init; }
        public int Currency { get; init; }
        public DateTime? DueDate { get; init; }
        public IReadOnlyList<InvoiceLineCreateDto> Lines { get; init; } = Array.Empty<InvoiceLineCreateDto>();
    }

    public record InvoiceLineCreateDto
    {
        public string Description { get; init; } = default!;
        public decimal Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public Guid? FeatureId { get; init; }
        public Guid? SubscriptionId { get; init; }
    }

    public record InvoiceUpdateDto
    {
        public int InvoiceState { get; init; } // örn Paid
    }

}
