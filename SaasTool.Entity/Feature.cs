using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class Feature : BaseEntity
    {
        public Guid AppId { get; set; }
        public App App { get; set; } = null!;

        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();
    }

}
