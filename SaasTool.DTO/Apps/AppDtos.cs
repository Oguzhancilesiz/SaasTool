using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Apps
{
    public record AppDto : AuditedDto
    {
        public string Code { get; init; } = default!;
        public string Name { get; init; } = default!;
        public bool IsEnabled { get; init; }
    }

    public record AppCreateDto
    {
        public string Code { get; init; } = default!;
        public string Name { get; init; } = default!;
        public bool IsEnabled { get; init; } = true;
    }

    public record AppUpdateDto
    {
        public string Name { get; init; } = default!;
        public bool IsEnabled { get; init; }
    }

}
