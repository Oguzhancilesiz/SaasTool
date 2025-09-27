using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.DTO.Apps;
using SaasTool.DTO.Common;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;

namespace SaasTool.Service.Concrete
{
    public sealed class AppService : IAppService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public AppService(IUnitOfWork uow, IMapper mapper) { _uow = uow; _mapper = mapper; }

        public async Task<Guid> CreateAsync(AppCreateDto dto, CancellationToken ct)
        {
            var entity = _mapper.Map<App>(dto);
            await _uow.Repository<App>().AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task UpdateAsync(Guid id, AppUpdateDto dto, CancellationToken ct)
        {
            var repo = _uow.Repository<App>();
            var entity = await repo.GetById(id) ?? throw new InvalidOperationException("App not found.");
            _mapper.Map(dto, entity);
            await repo.Update(entity);
            await _uow.SaveChangesAsync();
        }

        public async Task<AppDto?> GetAsync(Guid id, CancellationToken ct)
        {
            var entity = await _uow.Repository<App>().GetById(id);
            return entity is null ? null : _mapper.Map<AppDto>(entity);
        }

        public async Task<PagedResponse<AppDto>> ListAsync(PagedRequest req, CancellationToken ct)
        {
            var n = req.Normalize();
            var q = (await _uow.Repository<App>().GetAllActives()).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(n.Search))
                q = q.Where(x => x.Name.Contains(n.Search) || x.Code.Contains(n.Search));

            var total = await q.CountAsync(ct);
            var items = await q.OrderBy(x => x.AutoID)
                               .Skip((n.Page - 1) * n.PageSize)
                               .Take(n.PageSize)
                               .ProjectToType<AppDto>(_mapper.Config)
                               .ToListAsync(ct);

            return new(items, total, n.Page, n.PageSize);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var repo = _uow.Repository<App>();
            var entity = await repo.GetById(id);
            if (entity is null) return;
            await repo.Delete(entity);
            await _uow.SaveChangesAsync();
        }
    }
}
