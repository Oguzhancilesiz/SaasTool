using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SaasTool.Core.Abstracts;
using SaasTool.DTO.Apps;
using SaasTool.DTO.Common;
using SaasTool.Entity;
using SaasTool.Service.Abstracts;

namespace SaasTool.Service.Concrete;

public sealed class FeatureService : IFeatureService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public FeatureService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow; _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(FeatureCreateDto dto, CancellationToken ct)
    {
        var entity = _mapper.Map<Feature>(dto);
        await _uow.Repository<Feature>().AddAsync(entity);
        await _uow.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, FeatureUpdateDto dto, CancellationToken ct)
    {
        var repo = _uow.Repository<Feature>();
        var entity = await repo.GetById(id);
        if (entity is null) throw new InvalidOperationException("Feature not found.");
        _mapper.Map(dto, entity);
        await repo.Update(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task<FeatureDto?> GetAsync(Guid id, CancellationToken ct)
    {
        var entity = await _uow.Repository<Feature>().GetById(id);
        return entity is null ? null : _mapper.Map<FeatureDto>(entity);
    }

    public async Task<PagedResponse<FeatureDto>> ListAsync(Guid? appId, PagedRequest req, CancellationToken ct)
    {
        var q = await _uow.Repository<Feature>().GetAllActives();
        if (appId is not null) q = q.Where(x => x.AppId == appId);
        if (!string.IsNullOrWhiteSpace(req.Search))
            q = q.Where(x => x.Name.Contains(req.Search) || x.Code.Contains(req.Search));

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(x => x.AutoID)
                           .Skip((req.Page - 1) * req.PageSize)
                           .Take(req.PageSize)
                           .ProjectToType<FeatureDto>(_mapper.Config)
                           .ToListAsync(ct);

        return new(items, total, req.Page, req.PageSize);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var repo = _uow.Repository<Feature>();
        var entity = await repo.GetById(id);
        if (entity is null) return;
        await repo.Delete(entity);
        await _uow.SaveChangesAsync();
    }
}
