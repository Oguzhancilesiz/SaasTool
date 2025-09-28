using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.DTO.Apps;
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
    public sealed class AppPlanService : IAppPlanService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AppPlanService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow; _mapper = mapper;
        }

        public async Task<Guid> CreateAsync(AppPlanCreateDto dto, CancellationToken ct)
        {
            // Aynı App + Plan çifti varsa ikinciyi engelle
            var exists = await (await _uow.Repository<AppPlan>().GetAllActives())
                .AnyAsync(x => x.AppId == dto.AppId && x.PlanId == dto.PlanId, ct);
            if (exists) throw new InvalidOperationException("Bu plan zaten bu uygulamaya ekli.");

            var entity = _mapper.Map<AppPlan>(dto);
            await _uow.Repository<AppPlan>().AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task UpdateAsync(Guid id, AppPlanUpdateDto dto, CancellationToken ct)
        {
            var repo = _uow.Repository<AppPlan>();
            var entity = await repo.GetById(id) ?? throw new InvalidOperationException("AppPlan bulunamadı.");
            _mapper.Map(dto, entity);
            await repo.Update(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task<AppPlanDto?> GetAsync(Guid id, CancellationToken ct)
        {
            var q = (await _uow.Repository<AppPlan>().GetAllActives())
                    .Include(x => x.App)
                    .Include(x => x.Plan)
                    .AsNoTracking();

            var entity = await q.FirstOrDefaultAsync(x => x.Id == id, ct);
            return entity is null ? null : _mapper.Map<AppPlanDto>(entity);
        }

        public async Task<PagedResponse<AppPlanDto>> ListAsync(Guid? appId, Guid? planId, PagedRequest req, CancellationToken ct)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var size = req.PageSize <= 0 ? 20 : req.PageSize;

            var q = (await _uow.Repository<AppPlan>().GetAllActives())
                    .Include(x => x.App)
                    .Include(x => x.Plan)
                    .AsNoTracking();

            if (appId is not null) q = q.Where(x => x.AppId == appId);
            if (planId is not null) q = q.Where(x => x.PlanId == planId);

            var total = await q.CountAsync(ct);
            var items = await q.OrderBy(x => x.DisplayOrder ?? int.MaxValue).ThenBy(x => x.AutoID)
                               .Skip((page - 1) * size)
                               .Take(size)
                               .ProjectToType<AppPlanDto>(_mapper.Config)
                               .ToListAsync(ct);

            return new(items, total, page, size);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var repo = _uow.Repository<AppPlan>();
            var entity = await repo.GetById(id);
            if (entity is null) return;
            await repo.Delete(entity);
            await _uow.SaveChangesAsync();
        }
    }
}
