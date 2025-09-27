using SaasTool.DTO.Common;
using SaasTool.DTO.Orgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface IOrganizationService
    {
        Task<Guid> CreateAsync(OrganizationCreateDto dto, CancellationToken ct);
        Task UpdateAsync(Guid id, OrganizationUpdateDto dto, CancellationToken ct);
        Task<OrganizationDto?> GetAsync(Guid id, CancellationToken ct);
        Task<PagedResponse<OrganizationDto>> ListAsync(PagedRequest req, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
    }
}
