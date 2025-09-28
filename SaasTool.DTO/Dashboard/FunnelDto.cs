using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Dashboard
{
    public sealed class FunnelStageDto
    {
        public string Name { get; init; } = null!;
        public int Value { get; init; }
    }

    public sealed class FunnelDto
    {
        public List<FunnelStageDto> Stages { get; init; } = new();
    }

}
