using SaasTool.DTO.Apps;
using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface IFeatureService
    {
        Task<Guid> CreateAsync(FeatureCreateDto dto, CancellationToken ct);
        Task UpdateAsync(Guid id, FeatureUpdateDto dto, CancellationToken ct);
        Task<FeatureDto?> GetAsync(Guid id, CancellationToken ct);
        Task<PagedResponse<FeatureDto>> ListAsync(Guid? appId, PagedRequest req, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
    }

}
