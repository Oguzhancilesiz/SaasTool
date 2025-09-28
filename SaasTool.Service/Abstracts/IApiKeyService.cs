using SaasTool.DTO.Common;
using SaasTool.DTO.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface IApiKeyService
    {
        Task<ApiKeyCreatedDto> CreateAsync(ApiKeyCreateDto dto, CancellationToken ct);
        Task RevokeAsync(Guid id, CancellationToken ct);
        Task<PagedResponse<ApiKeyDto>> ListAsync(Guid? orgId, Guid? appId, PagedRequest req, CancellationToken ct);
    }

}
