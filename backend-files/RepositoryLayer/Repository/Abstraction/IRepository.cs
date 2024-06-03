using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Repository.Abstraction
{
    public interface IRepository<T, TId> where T : BaseEntity<TId>
    {
        Task<IEnumerable<T>> GetAllAsync();
        IQueryable<T> Query();

        Task<T> GetByIdAsync(TId id);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(T entity);
        Task<bool> DeleteByIdAsync(TId id);

        Task SaveChangesAsync();
        void SaveChanges();

    }
}
