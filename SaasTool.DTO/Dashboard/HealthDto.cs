using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Dashboard
{
    public sealed class HealthDto
    {
        public string Name { get; init; } = null!;   // DB, Cache, App
        public string Status { get; init; } = null!; // Healthy/Degraded/Unhealthy
        public int LatencyMs { get; init; }
    }

}
