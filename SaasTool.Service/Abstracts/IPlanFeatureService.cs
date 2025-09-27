using SaasTool.DTO.Common;
using SaasTool.DTO.Plans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface IPlanFeatureService
    {
        Task<Guid> CreateAsync(PlanFeatureCreateDto dto, CancellationToken ct);
        Task UpdateAsync(Guid id, PlanFeatureUpdateDto dto, CancellationToken ct);
        Task<PlanFeatureDto?> GetAsync(Guid id, CancellationToken ct);
        Task<PagedResponse<PlanFeatureDto>> ListAsync(Guid? planId, PagedRequest req, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
    }

}
