using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Orgs
{
    public record CustomerDto : AuditedDto
    {
        public Guid OrganizationId { get; init; }
        public string Name { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string? TaxNumber { get; init; }
        public string? BillingAddress { get; init; }
        public string? Country { get; init; }
        public string? City { get; init; }
    }

    public record CustomerCreateDto
    {
        public Guid OrganizationId { get; init; }
        public string Name { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string? TaxNumber { get; init; }
        public string? BillingAddress { get; init; }
        public string? Country { get; init; }
        public string? City { get; init; }
    }

    public record CustomerUpdateDto
    {
        public string Name { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string? TaxNumber { get; init; }
        public string? BillingAddress { get; init; }
        public string? Country { get; init; }
        public string? City { get; init; }
    }

}
