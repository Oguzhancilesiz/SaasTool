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
    public class PlanService : IPlanService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public PlanService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow; _mapper = mapper;
        }

        public async Task<Guid> CreateAsync(PlanCreateDto dto, CancellationToken ct)
        {
            var entity = _mapper.Map<Plan>(dto);
            await _uow.Repository<Plan>().AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task UpdateAsync(Guid id, PlanUpdateDto dto, CancellationToken ct)
        {
            var repo = _uow.Repository<Plan>();
            var plan = await repo.GetById(id);
            if (plan is null) throw new InvalidOperationException("Plan not found.");

            // Mapster: dto => entity
            _mapper.Map(dto, plan);
            await repo.Update(plan);
            await _uow.SaveChangesAsync();
        }

        public async Task<PlanDto?> GetAsync(Guid id, CancellationToken ct)
        {
            var plan = await _uow.Repository<Plan>().GetById(id);
            return plan is null ? null : _mapper.Map<PlanDto>(plan);
        }

        public async Task<PagedResponse<PlanDto>> ListAsync(PagedRequest req, CancellationToken ct)
        {
            var q = await _uow.Repository<Plan>().GetAllActives();
            if (!string.IsNullOrWhiteSpace(req.Search))
                q = q.Where(x => x.Name.Contains(req.Search) || x.Code.Contains(req.Search));

            var total = await q.CountAsync(ct);
            var items = await q.OrderBy(x => x.AutoID)
                               .Skip((req.Page - 1) * req.PageSize)
                               .Take(req.PageSize)
                               .ProjectToType<PlanDto>(_mapper.Config) // Mapster
                               .ToListAsync(ct);

            return new(items, total, req.Page, req.PageSize);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var repo = _uow.Repository<Plan>();
            var plan = await repo.GetById(id);
            if (plan is null) return;
            await repo.Delete(plan); // soft-delete
            await _uow.SaveChangesAsync();
        }
    }
}
