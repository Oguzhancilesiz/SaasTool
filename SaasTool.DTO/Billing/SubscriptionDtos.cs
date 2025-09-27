using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Billing
{
    public record SubscriptionItemDto : AuditedDto
    {
        public Guid SubscriptionId { get; init; }
        public Guid? FeatureId { get; init; }
        public decimal Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public int Currency { get; init; }
    }

    public record SubscriptionItemCreateDto
    {
        public Guid SubscriptionId { get; init; }
        public Guid? FeatureId { get; init; }
        public decimal Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public int Currency { get; init; }
    }

    public record SubscriptionItemUpdateDto
    {
        public decimal Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }

    public record SubscriptionDto : AuditedDto
    {
        public Guid OrganizationId { get; init; }
        public Guid AppId { get; init; }
        public Guid PlanId { get; init; }
        public Guid? CustomerId { get; init; }
        public int SubscriptionState { get; init; }   // enum int
        public DateTime StartsAt { get; init; }
        public DateTime? TrialEndsAt { get; init; }
        public DateTime? EndsAt { get; init; }
        public DateTime? CurrentPeriodStart { get; init; }
        public DateTime? CurrentPeriodEnd { get; init; }
        public bool CancelAtPeriodEnd { get; init; }
        public int Provider { get; init; }
        public string? ProviderCustomerId { get; init; }
        public string? ProviderSubscriptionId { get; init; }

        public IReadOnlyList<SubscriptionItemDto> Items { get; init; } = Array.Empty<SubscriptionItemDto>();
    }

    public record SubscriptionCreateDto
    {
        public Guid OrganizationId { get; init; }
        public Guid AppId { get; init; }
        public Guid PlanId { get; init; }
        public Guid? CustomerId { get; init; }
        public DateTime StartsAt { get; init; } = DateTime.UtcNow;
        public int? Provider { get; init; }
        public string? ProviderCustomerId { get; init; }
        public string? ProviderSubscriptionId { get; init; }
        public bool CancelAtPeriodEnd { get; init; } = false;

        public IReadOnlyList<SubscriptionItemCreateDto>? Items { get; init; }
    }

    public record SubscriptionUpdateDto
    {
        public Guid PlanId { get; init; }
        public bool CancelAtPeriodEnd { get; init; }
        public DateTime? EndsAt { get; init; }
    }

}
