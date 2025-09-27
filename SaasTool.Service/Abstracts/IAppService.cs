using SaasTool.DTO.Apps;
using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface IAppService
    {
        Task<Guid> CreateAsync(AppCreateDto dto, CancellationToken ct);
        Task UpdateAsync(Guid id, AppUpdateDto dto, CancellationToken ct);
        Task<AppDto?> GetAsync(Guid id, CancellationToken ct);
        Task<PagedResponse<AppDto>> ListAsync(PagedRequest req, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
    }
}
