using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Abstraction
{
    public interface IService<T1, T2, TId>
        where T1 : BaseEntity<TId>
        where T2 : class
    {
        Task<T2> AddAsync(T2 entity);
        Task<bool> DeleteAsync(T2 entity);
        Task<bool> DeleteById(TId id);
        Task<T2> UpdateAsync(T2 entity);
        Task<IEnumerable<T2>> GetAllAsync();
        Task<T2> GetAsync(int id);
    }
}
