using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.Service.Abstracts
{
    public interface IBaseService<TEntity, TAddEntity, TUpdateEntity>
    {
        Task Add(TAddEntity item);
        Task Update(TUpdateEntity item);
        Task Delete(Guid id);
        Task<List<TEntity>> GetAll();
        Task<TEntity> GetById(Guid id);
    }
}
