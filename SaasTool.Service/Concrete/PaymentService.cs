using MapsterMapper;
using SaasTool.Core.Abstracts;
using SaasTool.DTO.Billing;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Concrete
{
    public sealed class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
        public PaymentService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<Guid> CreateAsync(PaymentCreateDto dto, CancellationToken ct)
        {
            var e = _mapper.Map<Payment>(dto);
            await _uow.Repository<Payment>().AddAsync(e);
            await _uow.SaveChangesAsync();
            return e.Id;
        }
    }

}
