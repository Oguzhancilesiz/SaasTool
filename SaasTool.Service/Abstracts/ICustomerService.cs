using SaasTool.DTO.Common;
using SaasTool.DTO.Orgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface ICustomerService
    {
        Task<Guid> CreateAsync(CustomerCreateDto dto, CancellationToken ct);
        Task UpdateAsync(Guid id, CustomerUpdateDto dto, CancellationToken ct);
        Task<CustomerDto?> GetAsync(Guid id, CancellationToken ct);
        Task<PagedResponse<CustomerDto>> ListAsync(Guid? organizationId, PagedRequest req, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
    }
}
