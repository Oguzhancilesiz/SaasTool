using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.DTO.Billing;
using SaasTool.DTO.Common;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;

namespace SaasTool.Service.Concrete
{
    public sealed class UsageService : IUsageService
    {
        private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
        public UsageService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<Guid> CreateAsync(UsageRecordCreateDto dto, CancellationToken ct)
        {
            var e = _mapper.Map<UsageRecord>(dto);
            await _uow.Repository<UsageRecord>().AddAsync(e);
            await _uow.SaveChangesAsync();
            return e.Id;
        }

        public async Task<PagedResponse<UsageRecordDto>> ListAsync(Guid? subscriptionId, Guid? featureId, PagedRequest req, CancellationToken ct)
        {
            var n = req.Normalize();
            var q = (await _uow.Repository<UsageRecord>().GetAllActives()).AsNoTracking();
            if (subscriptionId is not null) q = q.Where(x => x.SubscriptionId == subscriptionId);
            if (featureId is not null) q = q.Where(x => x.FeatureId == featureId);

            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(x => x.PeriodStart)
                               .Skip((n.Page - 1) * n.PageSize)
                               .Take(n.PageSize)
                               .ProjectToType<UsageRecordDto>(_mapper.Config)
                               .ToListAsync(ct);
            return new(items, total, n.Page, n.PageSize);
        }
    }
}
