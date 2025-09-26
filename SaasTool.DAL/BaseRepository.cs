using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SaasTool.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DAL
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class, IEntity
    {
        private readonly IEFContext _context;
        private readonly DbSet<T> _table;

        public BaseRepository(IEFContext context)
        {
            _context = context;
            _table = _context.Set<T>();
        }
        public async Task AddAsync(T item)
        {
            try
            {
                await _table.AddAsync(item);
                //Logdb barışılı işlem mesajları
            }
            catch (Exception ex)
            {
                //logDb hata işlemleri yazılabilir
                throw new Exception(ex.Message);
            }
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                await _table.AddRangeAsync(entities);
                //Logdb barışılı işlem mesajları
            }
            catch (Exception ex)
            {
                //logDb hata işlemleri yazılabilir
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate = null, bool ignoreQueryFilter = false)
        {
            try
            {
                IQueryable<T> query = await GetDbSet();
                if (ignoreQueryFilter)
                {
                    query = query.IgnoreQueryFilters();

                }

                if (predicate is not null)
                {
                    query = query.Where(predicate);
                }

                var result = await query.AnyAsync();
                return result;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, bool ignoreQueryFilter = false)
        {
            try
            {
                var query = await GetDbSet();
                if (ignoreQueryFilter)
                {
                    query = query.IgnoreQueryFilters();
                }

                if (predicate is not null)
                {
                    query = query.Where(predicate);
                }

                int count = await query.CountAsync();

                return count;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public async Task Delete(T item)
        {
            try
            {
                var isComeFromDelete = true;
                item.Status = Core.Enums.Status.Deleted;
                await Update(item, isComeFromDelete);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public async Task<List<T>> GetAll()
        {
            return await _table.ToListAsync();
        }

        public async Task<IQueryable<T>> GetAllActives()
        {
            return _table.Where(x => x.Status == Core.Enums.Status.Active);
        }




        public async Task<IQueryable<T>> GetBy(Expression<Func<T, bool>> exp)
        {
            try
            {
                var query = await GetDbSet();
                query = query.Where(exp);


                return query;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public async Task<T> GetById(Guid id, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, Expression<Func<T, bool>> predicate = null, bool ignoreQueryFilter = false)
        {
            try
            {
                IQueryable<T> query = await GetDbSet();
                if (ignoreQueryFilter)
                {
                    query = query.IgnoreQueryFilters();
                }

                if (predicate is not null)
                {
                    query = query.Where(predicate);
                }
                if (include is not null)
                {
                    query = include(query);
                }

                var item = await query
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync();

                return item;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public async Task HardDeleteRangeAsync()
        {
            try
            {
                var result = await _table.IgnoreQueryFilters().Where(x => x.Status == Core.Enums.Status.Deleted).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> Save(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _context.SaveChangesAsync(cancellationToken);
                return result > 0 ? true : false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<decimal> SumAsync(Expression<Func<T, decimal>> selector, Expression<Func<T, bool>> predicate = null, bool ignoreQueryFilter = false)
        {
            try
            {
                var query = await GetDbSet();
                if (ignoreQueryFilter)
                {
                    query = query.IgnoreQueryFilters();
                }

                if (predicate is not null)
                {
                    query = query.Where(predicate);
                }

                decimal sum = await query.SumAsync(selector);

                return sum;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public virtual async Task Update(T item, bool isComeFromDelete = false)
        {
            try
            {
                _context.Entry(item).State = EntityState.Modified;
                if (isComeFromDelete)
                {
                    //Log tablosunda item update işleminin aslında Silme işlemi olduğunu yazmak için kullanılır.
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<IQueryable<T>> GetDbSet()
        {
            try
            {
                var list = _table.Where(x => x.Status != Core.Enums.Status.Deleted);

                return list;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public async Task<List<TResult>> GetAllAsync<TResult>(Expression<Func<T, TResult>> selector,
           Expression<Func<T, bool>> predicate = null,
           Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
           Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
           Func<IQueryable<T>, IQueryable<T>> skipTake = null,
           bool disableTracking = true, bool ignoreQueryFilter = false)
           where TResult : class
        {
            try
            {
                IQueryable<T> query = await GetDbSet();

                if (disableTracking)
                {
                    query = query.AsNoTracking();
                }

                if (include != null)
                {
                    query = include(query);
                }

                if (ignoreQueryFilter)
                {
                    query = query.IgnoreQueryFilters();
                }

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                if (skipTake != null)
                {
                    query = skipTake(query);
                }

                if (orderBy != null)
                {
                    query = orderBy(query);
                }

                var selectedQuery = query.Select(selector);

                var result = await selectedQuery.ToListAsync();


                return result;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
    }
}
