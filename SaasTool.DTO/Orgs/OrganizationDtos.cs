using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Orgs
{
    public record OrganizationDto : AuditedDto
    {
        public string Name { get; init; } = default!;
        public string? Slug { get; init; }
    }

    public record OrganizationCreateDto
    {
        public string Name { get; init; } = default!;
        public string? Slug { get; init; }
    }

    public record OrganizationUpdateDto
    {
        public string Name { get; init; } = default!;
        public string? Slug { get; init; }
    }

}
