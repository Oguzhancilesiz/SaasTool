using SaasTool.DTO.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface IPaymentService
    {
        Task<Guid> CreateAsync(PaymentCreateDto dto, CancellationToken ct);
    }
}
