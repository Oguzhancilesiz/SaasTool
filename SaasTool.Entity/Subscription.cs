using SaasTool.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    // Subscription.cs
    public class Subscription : BaseEntity
    {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public Guid AppId { get; set; }
        public App App { get; set; } = null!;

        public Guid PlanId { get; set; }
        public Plan Plan { get; set; } = null!;

        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // Eskiden: public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
        public SubscriptionStatus SubscriptionState { get; set; } = SubscriptionStatus.Pending;

        public DateTime StartsAt { get; set; } = DateTime.UtcNow;
        public DateTime? TrialEndsAt { get; set; }
        public DateTime? EndsAt { get; set; }

        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public bool CancelAtPeriodEnd { get; set; }

        public PaymentProvider Provider { get; set; } = PaymentProvider.Manual;
        public string? ProviderCustomerId { get; set; }
        public string? ProviderSubscriptionId { get; set; }

        public ICollection<SubscriptionItem> Items { get; set; } = new List<SubscriptionItem>();
        public ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
        public ICollection<SubscriptionCoupon> Coupons { get; set; } = new List<SubscriptionCoupon>();
    }

}
