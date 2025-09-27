using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Entity
{
    public class App : BaseEntity
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsEnabled { get; set; } = true;

        public ICollection<AppPlan> AppPlans { get; set; } = new List<AppPlan>();
        public ICollection<Feature> Features { get; set; } = new List<Feature>();
    }
}
