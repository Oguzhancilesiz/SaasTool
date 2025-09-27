using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Billing
{
    public record PaymentDto : AuditedDto
    {
        public Guid InvoiceId { get; init; }
        public int Provider { get; init; }
        public string? ProviderPaymentId { get; init; }
        public decimal Amount { get; init; }
        public int Currency { get; init; }
        public DateTime PaidAt { get; init; }
    }

    public record PaymentCreateDto
    {
        public Guid InvoiceId { get; init; }
        public int Provider { get; init; }
        public string? ProviderPaymentId { get; init; }
        public decimal Amount { get; init; }
        public int Currency { get; init; }
        public DateTime PaidAt { get; init; }
    }

}
