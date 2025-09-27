using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Security
{
    public record ApiKeyDto : AuditedDto
    {
        public Guid OrganizationId { get; init; }
        public Guid AppId { get; init; }
        public string Name { get; init; } = default!;
        public string KeyLast4 { get; init; } = default!; // Hash saklıyoruz, son 4 dışarı
        public DateTime? ExpiresAt { get; init; }
        public bool IsRevoked { get; init; }
    }

    public record ApiKeyCreateDto
    {
        public Guid OrganizationId { get; init; }
        public Guid AppId { get; init; }
        public string Name { get; init; } = default!;
        public DateTime? ExpiresAt { get; init; }
    }

    public record ApiKeyRotateDto
    {
        public DateTime? ExpiresAt { get; init; }
    }

}
