using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReminderApp.Data;
using ReminderApp.Entity;
using ReminderApp.Repository;
using Telegram.Bot;

namespace ReminderApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReminderController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult > CreateReminder()
        {
            return Ok();
        }

    }
}
