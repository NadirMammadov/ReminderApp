using Microsoft.EntityFrameworkCore;
using ReminderApp.Data;
using ReminderApp.Entity;
using ReminderApp.IRepository;

namespace ReminderApp.Repository
{
    public class ReminderRepository: IReminderRepository
    {
        private readonly ReminderDbContext _dbContext;

        public ReminderRepository(ReminderDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<List<Reminder>> GetAllAsync(long chatId)
        {
            var response =await _dbContext.Reminders.Where(x=>x.ChatId == chatId).ToListAsync();
            return  response;
        }
    }
}
