using Microsoft.EntityFrameworkCore;
using ReminderApp.Data;
using ReminderApp.Entity;
using ReminderApp.IRepository;
using System.Linq.Expressions;

namespace ReminderApp.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ReminderDbContext _dbContext;

        public Repository(ReminderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task  CreateAsync(T entity)
        {
           await _dbContext.Set<T>().AddAsync(entity);
           await _dbContext.SaveChangesAsync();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await  _dbContext.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task RemoveByCondition(Expression<Func<T, bool>> condition)
        {
            var entityToRemove = await _dbContext.Set<T>().FirstOrDefaultAsync(condition);
            if(entityToRemove != null)
            {
                _dbContext.Set<T>().Remove(entityToRemove);
               await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public Task UpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

       
    }
}
