using System.Linq.Expressions;

namespace TalepService.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity); // EF Core tracks changes, but explicit update might be useful
        Task DeleteAsync(T entity);
        Task<bool> SaveChangesAsync();
    }
}
