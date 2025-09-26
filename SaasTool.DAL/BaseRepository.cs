using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SaasTool.Core.Abstracts;
using System.Linq.Expressions;

public class BaseRepository<T> : IBaseRepository<T> where T : class, IEntity
{
    private readonly IEFContext _context;
    private readonly DbSet<T> _table;

    public BaseRepository(IEFContext context)
    {
        _context = context;
        _table = _context.Set<T>();
    }

    public Task AddAsync(T item) => _table.AddAsync(item).AsTask();
    public Task AddRangeAsync(IEnumerable<T> entities) => _table.AddRangeAsync(entities);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate = null, bool ignoreQueryFilter = false)
    {
        IQueryable<T> query = _table;
        if (ignoreQueryFilter) query = query.IgnoreQueryFilters();
        if (predicate is not null) query = query.Where(predicate);
        return await query.AnyAsync();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, bool ignoreQueryFilter = false)
    {
        IQueryable<T> query = _table;
        if (ignoreQueryFilter) query = query.IgnoreQueryFilters();
        if (predicate is not null) query = query.Where(predicate);
        return await query.CountAsync();
    }

    public Task<List<T>> GetAll() => _table.ToListAsync();

    public Task<IQueryable<T>> GetAllActives()
        => Task.FromResult(_table.Where(x => x.Status == SaasTool.Core.Enums.Status.Active).AsQueryable());

    public Task<IQueryable<T>> GetBy(Expression<Func<T, bool>> exp)
        => Task.FromResult(_table.Where(exp).AsQueryable());

    public async Task<T> GetById(Guid id,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        Expression<Func<T, bool>> predicate = null, bool ignoreQueryFilter = false)
    {
        IQueryable<T> query = _table;
        if (ignoreQueryFilter) query = query.IgnoreQueryFilters();
        if (predicate is not null) query = query.Where(predicate);
        if (include is not null) query = include(query);
        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task HardDeleteRangeAsync()
        => _table.IgnoreQueryFilters().Where(x => x.Status == SaasTool.Core.Enums.Status.Deleted).ExecuteDeleteAsync();

    public async Task<bool> Save(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken) > 0;

    public async Task<decimal> SumAsync(Expression<Func<T, decimal>> selector,
        Expression<Func<T, bool>> predicate = null, bool ignoreQueryFilter = false)
    {
        IQueryable<T> query = _table;
        if (ignoreQueryFilter) query = query.IgnoreQueryFilters();
        if (predicate is not null) query = query.Where(predicate);
        return await query.SumAsync(selector);
    }

    public Task Update(T item, bool isComeFromDelete = false)
    {
        _context.Entry(item).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task Delete(T item)
    {
        item.Status = SaasTool.Core.Enums.Status.Deleted;
        _context.Entry(item).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task<IQueryable<T>> GetDbSet()
        => Task.FromResult(_table.AsQueryable()); // Global filter zaten aktif
}
