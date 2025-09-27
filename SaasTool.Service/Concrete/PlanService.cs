using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.DTO.Common;
using SaasTool.DTO.Plans;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;

namespace SaasTool.Service.Concrete
{
    public class PlanService : IPlanService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public PlanService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

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
            var plan = await repo.GetById(id) ?? throw new InvalidOperationException("Plan not found.");
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
            var n = req.Normalize();
            var q = (await _uow.Repository<Plan>().GetAllActives()).AsNoTracking();
            if (!string.IsNullOrWhiteSpace(n.Search))
                q = q.Where(x => x.Name.Contains(n.Search) || x.Code.Contains(n.Search));

            var total = await q.CountAsync(ct);
            var items = await q.OrderBy(x => x.AutoID)
                               .Skip((n.Page - 1) * n.PageSize)
                               .Take(n.PageSize)
                               .ProjectToType<PlanDto>(_mapper.Config)
                               .ToListAsync(ct);

            return new(items, total, n.Page, n.PageSize);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var repo = _uow.Repository<Plan>();
            var plan = await repo.GetById(id);
            if (plan is null) return;
            await repo.Delete(plan);
            await _uow.SaveChangesAsync();
        }
    }
}
