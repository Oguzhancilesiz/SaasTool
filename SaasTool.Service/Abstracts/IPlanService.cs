using SaasTool.DTO.Common;
using SaasTool.DTO.Plans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface IPlanService
    {
        Task<Guid> CreateAsync(PlanCreateDto dto, CancellationToken ct);
        Task UpdateAsync(Guid id, PlanUpdateDto dto, CancellationToken ct);
        Task<PlanDto?> GetAsync(Guid id, CancellationToken ct);
        Task<PagedResponse<PlanDto>> ListAsync(PagedRequest req, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
    }
}
