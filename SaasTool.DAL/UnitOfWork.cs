using SaasTool.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IEFContext _context;
        private readonly Dictionary<string, object> _repositories = new();

        public UnitOfWork(IEFContext context) => _context = context;

        public IBaseRepository<T> Repository<T>() where T : class, IEntity
        {
            var key = typeof(T).FullName!;
            if (!_repositories.TryGetValue(key, out var repo))
            {
                repo = new BaseRepository<T>(_context);
                _repositories[key] = repo;
            }
            return (IBaseRepository<T>)repo;
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync(CancellationToken.None);
    }
}
