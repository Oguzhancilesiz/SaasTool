using SaasTool.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class PlanFeature : BaseEntity
    {
        public Guid PlanId { get; set; }
        public Plan Plan { get; set; } = null!;

        public Guid FeatureId { get; set; }
        public Feature Feature { get; set; } = null!;

        public FeatureLimitUnit Unit { get; set; } = FeatureLimitUnit.Count;
        public decimal? LimitValue { get; set; } // null = sınırsız
        public bool OveragesEnabled { get; set; } // limit aşımlarında ücretlendir
        public decimal? OverageUnitPrice { get; set; } // aşılan birim başı ücret
    }

}
