using SaasTool.DTO.Apps;
using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface IAppPlanService
    {
        Task<Guid> CreateAsync(AppPlanCreateDto dto, CancellationToken ct);
        Task UpdateAsync(Guid id, AppPlanUpdateDto dto, CancellationToken ct);
        Task<AppPlanDto?> GetAsync(Guid id, CancellationToken ct);
        Task<PagedResponse<AppPlanDto>> ListAsync(Guid? appId, Guid? planId, PagedRequest req, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
    }
}
