using Microsoft.EntityFrameworkCore;
using ReminderApp.Data;
using ReminderApp.IRepository;
using ReminderApp.Repository;
using ReminderApp.Services;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ReminderDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
}, ServiceLifetime.Scoped);
builder.Services.AddScoped<IReminderRepository, ReminderRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<TelegramService>();
builder.Services.AddScoped<ReminderService>();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;

    try
    {
        // ReminderService'i al
        var reminderService = services.GetRequiredService<TelegramService>();

        // StartReceiving methodunu çağır
        reminderService.StartReceiving();
    }
    catch (Exception ex)
    {
        // Hata yönetimi
        Console.WriteLine("Hata: " + ex.Message);
    }

app.Run();
