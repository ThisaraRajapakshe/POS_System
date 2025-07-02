
using Microsoft.EntityFrameworkCore;
using POS_System.Data;

namespace POS_System.Repositories.Implementation
{
    public class BaseRepository<T, TKey> : IBaseRepository<T, TKey> where T : class
    {
        private readonly PosSystemDbContext dbContext;
        private readonly DbSet<T> dbSet;

        public BaseRepository(PosSystemDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<T>();
        }

        public async Task<T> CreateAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(TKey id)
        {
            var entity = await dbSet.FindAsync(id);
            if (entity != null)
            {
                dbSet.Remove(entity);
                await dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }


        public async Task<T?> GetAsync(TKey id) => await dbSet.FindAsync(id);
        public async Task<List<T>> GetAsync() => await dbSet.ToListAsync();

        public async Task<T?> UpdateAsync(T entity, TKey id)
        {
            var existingEntity = await dbSet.FindAsync(id);
            if (existingEntity == null)
            {
                return null;
            }

            //dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
            //Chatgpt
            var entry = dbContext.Entry(existingEntity);
            var entityEntry = dbContext.Entry(entity);

            foreach (var property in entry.Properties)
            {
                // Skip the primary key property
                if (!property.Metadata.IsPrimaryKey())
                {
                    property.CurrentValue = entityEntry.Property(property.Metadata.Name).CurrentValue;
                }
            }

            await dbContext.SaveChangesAsync();
            return existingEntity;
        }
    }
}
