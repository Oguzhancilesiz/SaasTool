using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Dashboard
{
    public sealed class RecentEventDto
    {
        public DateTime Ts { get; init; }        // UTC
        public string Provider { get; init; } = null!;
        public string EventType { get; init; } = null!;
        public bool IsProcessed { get; init; }
        public string? ProcessNote { get; init; }
    }

}
