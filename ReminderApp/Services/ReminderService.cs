using ReminderApp.Entity;
using ReminderApp.IRepository;

namespace ReminderApp.Services
{
    public class ReminderService
    {
        private readonly IReminderRepository _reminderRepository;
        private readonly IRepository<Reminder> _repository;

        public ReminderService(IReminderRepository reminderRepository, IRepository<Reminder> repository)
        {
            _reminderRepository = reminderRepository;
            _repository = repository;
        }
        public async Task<List<Reminder>> CheckReminder(DateTime dateTime)
        {
            return await _reminderRepository.CheckReminder(dateTime);
        }
        public async Task<List<Reminder>> GetAllAsync(long chatId)
        {
            return await _reminderRepository.GetAllAsync(chatId);
        }
        public async Task CreateReminder(Reminder reminder)
        {
            await _repository.CreateAsync(reminder);
        }
        public async Task RemoveByCondition(long chatId, int reminderId)
        {
            await _repository.RemoveByCondition(x => x.ChatId == chatId && x.Id == reminderId);
        }
    }
}
