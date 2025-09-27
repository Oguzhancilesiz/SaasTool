using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.DTO.Common;
using SaasTool.DTO.Plans;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Concrete
{
    public sealed class PlanFeatureService : IPlanFeatureService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public PlanFeatureService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<Guid> CreateAsync(PlanFeatureCreateDto dto, CancellationToken ct)
        {
            var e = _mapper.Map<PlanFeature>(dto);
            await _uow.Repository<PlanFeature>().AddAsync(e);
            await _uow.SaveChangesAsync();
            return e.Id;
        }

        public async Task UpdateAsync(Guid id, PlanFeatureUpdateDto dto, CancellationToken ct)
        {
            var repo = _uow.Repository<PlanFeature>();
            var e = await repo.GetById(id);
            if (e is null) throw new InvalidOperationException("PlanFeature not found.");
            _mapper.Map(dto, e);
            await repo.Update(e);
            await _uow.SaveChangesAsync();
        }

        public async Task<PlanFeatureDto?> GetAsync(Guid id, CancellationToken ct)
        {
            var e = await _uow.Repository<PlanFeature>().GetById(id);
            return e is null ? null : _mapper.Map<PlanFeatureDto>(e);
        }

        public async Task<PagedResponse<PlanFeatureDto>> ListAsync(Guid? planId, PagedRequest req, CancellationToken ct)
        {
            var q = await _uow.Repository<PlanFeature>().GetAllActives();
            if (planId is not null) q = q.Where(x => x.PlanId == planId);

            var total = await q.CountAsync(ct);
            var items = await q.OrderBy(x => x.AutoID)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ProjectToType<PlanFeatureDto>(_mapper.Config)
                .ToListAsync(ct);

            return new(items, total, req.Page, req.PageSize);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var repo = _uow.Repository<PlanFeature>();
            var e = await repo.GetById(id);
            if (e is null) return;
            await repo.Delete(e);
            await _uow.SaveChangesAsync();
        }
    }

}
