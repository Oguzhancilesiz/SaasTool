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
        private readonly Dictionary<string, dynamic> _repositoryDictionary;
        public UnitOfWork(IEFContext context)
        {
            _context = context;
            _repositoryDictionary = new Dictionary<string, dynamic>();
        }
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync(new CancellationToken());
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public IBaseRepository<T> Repository<T>() where T : class, IEntity
        {
            var entityName = typeof(T).Name;

            var repositoryCreated = _repositoryDictionary.ContainsKey(entityName);
            if (!repositoryCreated)
            {
                var newRepository = new BaseRepository<T>(_context);
                _repositoryDictionary.Add(entityName, newRepository);
            }

            return _repositoryDictionary[entityName];
        }
    }
}
