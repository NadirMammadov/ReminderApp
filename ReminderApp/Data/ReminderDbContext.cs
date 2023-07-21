using Microsoft.EntityFrameworkCore;
using ReminderApp.Entity;

namespace ReminderApp.Data
{
    public class ReminderDbContext : DbContext
    {
        public DbSet<Reminder> Reminders { get; set; }

        public ReminderDbContext(DbContextOptions<ReminderDbContext> options)
           : base(options)
        {

        }
    }
}
