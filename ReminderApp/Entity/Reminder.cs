namespace ReminderApp.Entity
{
    public class Reminder
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime DateTime { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public long ChatId { get; set; }
    }
}
