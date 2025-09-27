using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Orgs
{
    public record UserOrganizationDto : AuditedDto
    {
        public Guid OrganizationId { get; init; }
        public Guid UserId { get; init; }
        public string Role { get; init; } = default!;
        public DateTime JoinedAt { get; init; }
    }

    public record UserOrganizationCreateDto
    {
        public Guid OrganizationId { get; init; }
        public Guid UserId { get; init; }
        public string Role { get; init; } = default!;
    }

    public record UserOrganizationUpdateDto
    {
        public string Role { get; init; } = default!;
    }

}
