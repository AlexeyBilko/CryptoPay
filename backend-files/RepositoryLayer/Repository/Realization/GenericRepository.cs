using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Repository.Abstraction;
using RepositoryLayer.ApplicationDb;

namespace RepositoryLayer.Repository.Realization
{
    public abstract class GenericRepository<T, TKey> : IRepository<T, TKey> where T : BaseEntity<TKey>
    {
        protected ApplicationDbContext context;
        private DbSet<T> table;

        public GenericRepository(ApplicationDbContext context)
        {
            this.context = context;
            table = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await table.ToListAsync();
        }

        public IQueryable<T> Query()
        {
            return table.AsQueryable();
        }

        public async Task<T> GetByIdAsync(TKey id)
        {
            return await table.FindAsync(id);
        }

        public async Task<T> CreateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            await table.AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            table.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
            await SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            table.Remove(entity);
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByIdAsync(TKey id)
        {
            var entity = await table.FindAsync(id);
            if (entity != null)
            {
                table.Remove(entity);
                await SaveChangesAsync();
                return true;
            }
            return false;
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
