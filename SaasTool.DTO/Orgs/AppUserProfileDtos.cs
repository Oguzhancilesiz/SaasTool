using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Orgs
{
    public record AppUserProfileDto : AuditedDto
    {
        public Guid OrganizationId { get; init; }
        public Guid AppId { get; init; }
        public Guid UserId { get; init; }
        public string? DisplayName { get; init; }
        public string? PreferencesJson { get; init; }
        public bool IsBlocked { get; init; }
    }

    public record AppUserProfileCreateDto
    {
        public Guid OrganizationId { get; init; }
        public Guid AppId { get; init; }
        public Guid UserId { get; init; }
        public string? DisplayName { get; init; }
        public string? PreferencesJson { get; init; }
    }

    public record AppUserProfileUpdateDto
    {
        public string? DisplayName { get; init; }
        public string? PreferencesJson { get; init; }
        public bool IsBlocked { get; init; }
    }

}
