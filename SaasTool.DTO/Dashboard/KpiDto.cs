using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Dashboard
{
    public sealed class KpiDto
    {
        public decimal RevenueToday { get; init; }
        public decimal RevenueMTD { get; init; }
        public int NewCustomers7d { get; init; }
        public int ActiveSubscriptions { get; init; }
        public decimal Mrr { get; init; }
        public decimal ChurnRate { get; init; } // 0..1
    }

}
