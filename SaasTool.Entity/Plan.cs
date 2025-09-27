using SaasTool.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class Plan : BaseEntity
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Currency Currency { get; set; } = Currency.TRY;
        public decimal Price { get; set; }
        public BillingPeriod BillingPeriod { get; set; } = BillingPeriod.Monthly;
        public bool IsPublic { get; set; } = true;
        public int TrialDays { get; set; } = 0; // ücretsiz deneme

        public ICollection<AppPlan> AppPlans { get; set; } = new List<AppPlan>();
        public ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();
    }

}
