using SaasTool.DTO.Billing;
using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface IUsageService
    {
        Task<Guid> CreateAsync(UsageRecordCreateDto dto, CancellationToken ct);
        Task<PagedResponse<UsageRecordDto>> ListAsync(Guid? subscriptionId, Guid? featureId, PagedRequest req, CancellationToken ct);
    }
}
