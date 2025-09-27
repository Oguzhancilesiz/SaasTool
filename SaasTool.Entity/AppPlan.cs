using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class AppPlan : BaseEntity
    {
        public Guid AppId { get; set; }
        public App App { get; set; } = null!;

        public Guid PlanId { get; set; }
        public Plan Plan { get; set; } = null!;

        public bool IsEnabled { get; set; } = true;
        public int? DisplayOrder { get; set; }
    }

}
