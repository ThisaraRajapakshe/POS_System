namespace POS_System.Repositories
{
    public interface IBaseRepository<T, TKey> where T : class
    {
        Task<T?> GetAsync(TKey id);
        Task<List<T>> GetAsync();

        Task<T> CreateAsync(T entity);
        Task<T?> UpdateAsync(T entity, TKey id);
        Task<bool> DeleteAsync(TKey id);
       
    }
}
