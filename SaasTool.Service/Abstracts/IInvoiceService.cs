using SaasTool.DTO.Billing;
using SaasTool.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface IInvoiceService
    {
        Task<Guid> CreateAsync(InvoiceCreateDto dto, CancellationToken ct);
        Task<InvoiceDto?> GetAsync(Guid id, CancellationToken ct);
        Task<PagedResponse<InvoiceDto>> ListAsync(Guid? organizationId, PagedRequest req, CancellationToken ct);
    }
}
