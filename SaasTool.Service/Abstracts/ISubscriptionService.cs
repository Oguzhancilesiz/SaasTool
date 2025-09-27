using SaasTool.DTO.Billing;
using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface ISubscriptionService
    {
        Task<Guid> CreateAsync(SubscriptionCreateDto dto, CancellationToken ct);
        Task<SubscriptionDto?> GetAsync(Guid id, CancellationToken ct);
        Task<PagedResponse<SubscriptionDto>> ListAsync(Guid? organizationId, PagedRequest req, CancellationToken ct);
    }
}
