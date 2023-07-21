using ReminderApp.Entity;
using System.Linq.Expressions;

namespace ReminderApp.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task CreateAsync(T entity);
        Task UpdateAsync(T entity);
        Task RemoveAsync(int Id);
        Task RemoveByCondition(Expression<Func<T, bool>> condition);
    }
}
