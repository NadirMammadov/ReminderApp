using ReminderApp.Entity;

namespace ReminderApp.IRepository
{
    public interface IReminderRepository 
    {
        Task<List<Reminder>> GetAllAsync(long chatId);
    }
}
