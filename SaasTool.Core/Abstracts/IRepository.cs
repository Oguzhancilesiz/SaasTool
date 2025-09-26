using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Core.Abstracts
{
    public interface IRepository<T> where T : IEntity, new()
    {
        void Add(T item);
        void Update(T item);
        void Delete(T item);

        T GetById(Guid id);
        T GetBy(Expression<Func<T, bool>> expression);
        IQueryable<T> GetAllBy(Expression<Func<T, bool>> expression);
        IEnumerable<T> GetAll();
    }
}
