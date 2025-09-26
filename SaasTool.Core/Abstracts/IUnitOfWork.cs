using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Core.Abstracts
{
    public interface IUnitOfWork
    {
        IBaseRepository<T> Repository<T>() where T : class, IEntity;
        Task<int> SaveChangesAsync();
    }
}
