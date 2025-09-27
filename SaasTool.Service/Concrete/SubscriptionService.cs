using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.DTO.Billing;
using SaasTool.DTO.Common;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Concrete
{
    public sealed class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
        public SubscriptionService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<Guid> CreateAsync(SubscriptionCreateDto dto, CancellationToken ct)
        {
            var e = _mapper.Map<Subscription>(dto);
            await _uow.Repository<Subscription>().AddAsync(e);

            // Items
            if (dto.Items is { Count: > 0 })
            {
                foreach (var i in dto.Items)
                {
                    var item = _mapper.Map<SubscriptionItem>(i);
                    item.SubscriptionId = e.Id;
                    await _uow.Repository<SubscriptionItem>().AddAsync(item);
                }
            }

            await _uow.SaveChangesAsync();
            return e.Id;
        }

        public async Task<SubscriptionDto?> GetAsync(Guid id, CancellationToken ct)
        {
            var sub = await (await _uow.Repository<Subscription>().GetAllActives())
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
            return sub is null ? null : _mapper.Map<SubscriptionDto>(sub);
        }

        public async Task<PagedResponse<SubscriptionDto>> ListAsync(Guid? organizationId, PagedRequest req, CancellationToken ct)
        {
            var q = await _uow.Repository<Subscription>().GetAllActives(); // IQueryable<Subscription>

            if (organizationId is not null)
                q = q.Where(x => x.OrganizationId == organizationId);

            var total = await q.CountAsync(ct);

            var items = await q.OrderBy(x => x.AutoID)
                               .Skip((req.Page - 1) * req.PageSize)
                               .Take(req.PageSize)
                               .Include(x => x.Items)                                   // Include'u burada ekle
                               .ProjectToType<SubscriptionDto>(_mapper.Config)          // Mapster projection
                               .ToListAsync(ct);

            return new(items, total, req.Page, req.PageSize);
        }

    }
}
