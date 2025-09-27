using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Apps
{
    public record FeatureDto : AuditedDto
    {
        public Guid AppId { get; init; }
        public string Code { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
    }

    public record FeatureCreateDto
    {
        public Guid AppId { get; init; }
        public string Code { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
    }

    public record FeatureUpdateDto
    {
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
    }

}
