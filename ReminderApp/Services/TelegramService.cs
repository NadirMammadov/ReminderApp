using Microsoft.SqlServer.Server;
using ReminderApp.Data;
using ReminderApp.Entity;
using ReminderApp.Interfaces;
using ReminderApp.IRepository;
using ReminderApp.Repository;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ReminderApp.Services
{
    public class TelegramService : IReminderService
    {
        private TelegramBotClient _botClient;
        private Dictionary<long, int> _userDialogSteps;
        private Reminder reminder = new Reminder();
        private ReminderService _reminderService;
        private readonly Timer timer;
        private int year;
        private int month;
        private int day;
        private string choose;
        public TelegramService(ReminderService reminderService)
        {
            _botClient = new TelegramBotClient("6254497016:AAHrrnM0OmN77Gne90G6lFmfVFytUx617Dg");
            _botClient.OnUpdate += BotClient_OnMessage;
            _userDialogSteps = new Dictionary<long, int>();
            _reminderService = reminderService;
            timer = new Timer(CheckRemindersAndSendMessages, null, 0, 60000);
        }
        public async void CheckRemindersAndSendMessages(object state)
        {
            DateTime dateTime = DateTime.Now;
            var reminders =  await _reminderService.CheckReminder(dateTime);
            foreach(var reminder in reminders)
            {
                await SendMessage(reminder.ChatId, "<b>Salam ümüd edirəm işlərin qaydasındadır 🙂\nSənə bir xatırlatma mesajım var 🔔</b>", ParseMode.Html);
                await SendMessage(reminder.ChatId, $"<b>Xatırlatma mesajınız:\n</b>{reminder.Text}",ParseMode.Html);
                await SendMenuButton(reminder.ChatId);
                await _reminderService.RemoveByCondition(reminder.ChatId, reminder.Id);
            }
        }
        public void StartReceiving()
        {
            _botClient.StartReceiving();
        }

        public void StopReceiving()
        {
            _botClient.StopReceiving();
        }
        
        
        public async Task SendMessage(long chatId, string message,ParseMode mode= ParseMode.Default,IReplyMarkup? replyKeyboard=null)
        {
            await _botClient.SendTextMessageAsync(chatId,message,mode,replyMarkup: replyKeyboard);
        }
        public async Task SendMenuButton(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(
                new[]
                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Xatirlatma yarat"),
                                            InlineKeyboardButton.WithCallbackData("Xatirlatmalara bax")
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Bot haqqında"),
                                            InlineKeyboardButton.WithCallbackData("Bizimlə əlaqə")
                                        }
                    }
            );
            ResetUserDialogStep(chatId);
            await _botClient.SendTextMessageAsync(chatId, "Lütfen bir seçenek seçin:", replyMarkup: inlineKeyboard);
        }
       
        public async void BotClient_OnMessage(object sender, UpdateEventArgs e)
        {
            var update = e.Update;
            
            if (update.Message!=null)
            {
                var message = update.Message;
                var chatId = update.Message.Chat.Id;
                var text = message.Text;
                if (text == "/start" || text == "/menu")
                {
                    await SendMenuButton(chatId);
                }
                else if (text == "Ləğv et 🚫")
                {
                    ProcessCancel(chatId);
                    await SendMessage(chatId, $"<b>Leğv edildi.</b>", ParseMode.Html);
                    await SendMenuButton(chatId);
                }
                else
                {
                   await HandleUserResponse(message);
                }
            }
            else if (update.CallbackQuery !=null)
            {
               await BotClient_OnCallbackQuery(update.CallbackQuery);
            }
           
           
        }
        
        public async Task HandleUserResponse(Message message)
        {
            var chatId = message.Chat.Id;
            int userStep = GetUserDialogStep(chatId);
                switch (userStep)
                {
                    case 1:
                        await ReminderCreateProcessOne(chatId, message.Text);
                        break;
                    case 2:
                        await ReminderCreateProcessTwo(chatId, message.Text);
                        break;
                    case 3:
                        await ReminderCreateProcessThree(chatId, message.Text);
                        break;
                    case 4:
                        await ReminderCreateProcessFour(chatId, message.Text);
                        break;
                    case 5:
                        await ReminderCreateProcessFive(chatId, message.Text);
                        break;
                default:
                        await SendMessage(chatId, "Bilinməyən mətn daxil etdiniz.");
                        ResetUserDialogStep(chatId);
                        break;
                }
        }
        public async Task BotClient_OnCallbackQuery(CallbackQuery e)
        {

            var chatId = e.Message.Chat.Id;
            var data = e.Data;
            if (data == "Xatirlatma yarat")
            {
                choose = data;
                int userStep = GetUserDialogStep(chatId);
                switch (userStep)
                {
                    case 0:
                        ReminderCreateProcessZero(chatId);
                        break;
                    default:
                        await SendMessage(chatId, "Sehv");
                        break;
                }
            }
            else if (data == "Xatirlatmalara bax")
            {
                List<Reminder> reminders = await _reminderService.GetAllAsync(chatId);
                if (reminders.Count == 0)
                {
                    await SendMessage(chatId, $"<b>Xatırlatma mesajınınz yoxdur 📝</b>", ParseMode.Html);
                    await SendMenuButton(chatId);
                }
                else
                {
                    foreach (var reminder in reminders)
                    {
                        var inlineKeyboard = new InlineKeyboardMarkup(
                       new[]
                           {
                                            new[]
                                            {
                                                InlineKeyboardButton.WithCallbackData("Sil 🗑️",$"remove_{reminder.Id.ToString()}")
                                            }
                           });
                        await SendMessage(chatId, $"<b>Xatırlatma mesajı 📝</b>\n<i>{reminder.Text}</i>\n<b>Xatırlatma tarixi  ⏰</b>\n<i>{reminder.DateTime.ToString("dd/MM/yyyy HH:mm")} </i>", ParseMode.Html, inlineKeyboard);
                    }
                }

            }
            else if (data.StartsWith("remove_"))
            {
                int reminderId = int.Parse(e.Data.Substring(7));
                await _reminderService.RemoveByCondition(chatId, reminderId);
                await _botClient.SendTextMessageAsync(chatId, $"Xatırlatma silindi. 👌");
                await SendMenuButton(chatId);
            }
            else if (data.StartsWith("year_"))
            {
                await ReminderCreateProcessTwo(chatId, data.Substring(5));

            }
            else if (data.StartsWith("month_"))
            {
                await ReminderCreateProcessThree(chatId, data.Substring(6));

            }
            else if (data.StartsWith("day_"))
            {
                await ReminderCreateProcessFour(chatId, data.Substring(4));
            }
            else if(data == "Bot haqqında")
            {
                await SendMessage(chatId, "<b>ReminderNZbot</b> sizi salamlayır. \n Məndən istifadə ederək xatırlatmalar yarada bilərsiz :)",ParseMode.Html);
            }
            else if (data == "Bizimlə əlaqə")
            {
                await SendMessage(chatId, "<b>ReminderNZbot</b> sizi salamlayır\nBizimlə əlaqə: 0708538060\n<b>Developer by Nadir</b>", ParseMode.Html);
            }
            else if (data == "Ləğv et 🚫")
            {
                ProcessCancel(chatId);
                await SendMessage(chatId, $"<b>Leğv edildi.</b>", ParseMode.Html);
                await SendMenuButton(chatId);
            }
                
                
            else
            {
                await SendMessage(chatId, $"-----");

            }
        }
        private void ProcessCancel(long chatId)
        {
            reminder = new Reminder();
            ResetUserDialogStep(chatId);
        }
        private async Task ReminderCreateProcessZero(long chatId)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
                        {
                            new []
                            {
                                new KeyboardButton("Ləğv et 🚫")
                            }
                        })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
            await SendMessage(chatId, "<b>Xatırlatma mesajınızı yazın 📝</b>", ParseMode.Html,replyKeyboard);
            UpdateUserDialogStep(chatId, 1);
        }
        private async Task ReminderCreateProcessOne(long chatId,string text)
        {
            reminder.Text = text;
            var dateTime = DateTime.Now.Year.ToString();
            var dateTimeAddYear = DateTime.Now.AddYears(1).Year.ToString();
            var inlineKeyboard = new InlineKeyboardMarkup(
                new[]
                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData(dateTime,$"year_{dateTime}"),
                                            InlineKeyboardButton.WithCallbackData(dateTimeAddYear,$"year_{dateTimeAddYear}")
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Ləğv et 🚫")
                                        },
                    }
            );

            await SendMessage(chatId, "<b>Xatırlatmanı almağ istədiyiniz ili seçin</b> 📅", ParseMode.Html, inlineKeyboard);
           
            UpdateUserDialogStep(chatId, 2);
        }
        private async Task ReminderCreateProcessTwo(long chatId, string year)
        {
            try
            {
                this.year = int.Parse(year);
                var inlineKeyboard = new InlineKeyboardMarkup(
                    new[]
                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Yanvar",$"month_1"),
                                            InlineKeyboardButton.WithCallbackData("Fevral",$"month_2"),
                                            InlineKeyboardButton.WithCallbackData("Mart",$"month_3"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Aprel",$"month_4"),
                                            InlineKeyboardButton.WithCallbackData("May",$"month_5"),
                                            InlineKeyboardButton.WithCallbackData("Iyun",$"month_6"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Iyul",$"month_7"),
                                            InlineKeyboardButton.WithCallbackData("Avqust",$"month_8"),
                                            InlineKeyboardButton.WithCallbackData("Sentyabr",$"month_9"),

                                        },new[]
                                        {
                                             InlineKeyboardButton.WithCallbackData("Oktaybr",$"month_10"),
                                            InlineKeyboardButton.WithCallbackData("Noyabr",$"month_11"),
                                            InlineKeyboardButton.WithCallbackData("Dekabr",$"month_12"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Ləğv et 🚫")
                                        },
                    }
                );
                await SendMessage(chatId, "<b>Xatırlatmanı almağ istədiyiniz ayı seçin</b> 🌙", ParseMode.Html,  inlineKeyboard);
                
                UpdateUserDialogStep(chatId, 3);
            }
            catch (Exception)
            {
                await SendMessage(chatId, "<b>⚠️ Xatırlatmanı almağ istədiyiniz ili seçin və ya ləğv edin.⚠️</b> ", ParseMode.Html);
                await ReminderCreateProcessOne(chatId, reminder.Text);
            }
        }
        private async Task ReminderCreateProcessThree(long chatId, string month )
        {
            try
            {
                this.month = int.Parse(month);
                if(DateTime.Now.Month<=this.month)
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(
                                        new[]
                                            {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("1","day_1"),
                                            InlineKeyboardButton.WithCallbackData("2","day_2"),
                                            InlineKeyboardButton.WithCallbackData("3","day_3"),
                                            InlineKeyboardButton.WithCallbackData("4","day_4"),
                                            InlineKeyboardButton.WithCallbackData("5","day_5"),
                                            InlineKeyboardButton.WithCallbackData("6","day_6")
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("7","day_7"),
                                            InlineKeyboardButton.WithCallbackData("8","day_8"),
                                            InlineKeyboardButton.WithCallbackData("9","day_9"),
                                            InlineKeyboardButton.WithCallbackData("10","day_10"),
                                            InlineKeyboardButton.WithCallbackData("11","day_11"),
                                            InlineKeyboardButton.WithCallbackData("12","day_12")

                                        },new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("13","day_13"),
                                            InlineKeyboardButton.WithCallbackData("14","day_14"),
                                            InlineKeyboardButton.WithCallbackData("15","day_15"),
                                            InlineKeyboardButton.WithCallbackData("16","day_16"),
                                            InlineKeyboardButton.WithCallbackData("17","day_17"),
                                            InlineKeyboardButton.WithCallbackData("18","day_18")
                                        },new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("19","day_19"),
                                            InlineKeyboardButton.WithCallbackData("20","day_20"),
                                            InlineKeyboardButton.WithCallbackData("21","day_21"),
                                            InlineKeyboardButton.WithCallbackData("22","day_22"),
                                            InlineKeyboardButton.WithCallbackData("23","day_23"),
                                            InlineKeyboardButton.WithCallbackData("24","day_24")
                                        }
                                        ,new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("25","day_25"),
                                            InlineKeyboardButton.WithCallbackData("26","day_26"),
                                            InlineKeyboardButton.WithCallbackData("27","day_27"),
                                            InlineKeyboardButton.WithCallbackData("28","day_28"),
                                            InlineKeyboardButton.WithCallbackData("29","day_29"),
                                            InlineKeyboardButton.WithCallbackData("30","day_30")
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("31","day_31")
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Ləğv et 🚫")
                                        },
                                            }
                                    );
                    await SendMessage(chatId, $"<b>Xatırlatma gününü seçin</b> 🌞", ParseMode.Html, inlineKeyboard);
                    UpdateUserDialogStep(chatId, 4);
                }
                else
                {
                    await SendMessage(chatId, $"<b>Təəsüfki keçmiş aylara xatırlatma göndərə bilmirik 😔 </b>", ParseMode.Html);
                    await ReminderCreateProcessTwo(chatId, this.year.ToString());
                }

            }
            catch (Exception)
            {
                await SendMessage(chatId, $"<b>⚠️ Xatırlatma ayını seçin və ya leğv edin</b> ⚠️", ParseMode.Html);
                await ReminderCreateProcessTwo(chatId, this.year.ToString());
            }
           

        }
        private async Task ReminderCreateProcessFour(long chatId, string day)
        {
            try
            {
                this.day = int.Parse(day);
                try
                {
                    DateTime testTime = new DateTime(year, month, int.Parse(day));
                    ;
                }
                catch (Exception)
                {
                    await SendMessage(chatId, $"<b>⚠️ Xatırlatma tarixi düzgün deyil ele bir tarix mövcud deyil.</b> ⚠️", ParseMode.Html);
                    await ReminderCreateProcessThree(chatId, this.month.ToString());
                }
                if( int.Parse(day) >= DateTime.Now.Day)
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(
                            new[]
                                {
                               new[]
                               {
                                   InlineKeyboardButton.WithCallbackData("Ləğv et 🚫")
                               },
                                }
                    );
                    await SendMessage(chatId, $"<b>Xatırlatma saatını yazın</b> 🕓\n <b>⚠️ Diqqet saat <i>00:00</i> formatında yazılmalıdır. ⚠️</b>", ParseMode.Html, inlineKeyboard);
                    UpdateUserDialogStep(chatId, 5);
                }
                else
                {
                    await SendMessage(chatId, $"<b>⚠️ Zamanda işinlanma edə bilmirik :(\nKeçmiş bir tarixi seçdiniz yeniden seçin</b> ⚠️", ParseMode.Html);
                    await ReminderCreateProcessThree(chatId, this.month.ToString());
                    
                }
                
            }
            catch (Exception)
            {
                await SendMessage(chatId, $"<b>⚠️ Xatırlatma gününü seçin və ya ləğv edin</b> ⚠️", ParseMode.Html);
                await ReminderCreateProcessThree(chatId, this.month.ToString());
            }
            
        }
        private async Task ReminderCreateProcessFive(long chatId, string hour)
        {
            string format = "HH:mm";
            DateTime time;
            if (DateTime.TryParseExact(hour, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            {
                if(new DateTime(this.year, this.month, this.day, time.Hour, time.Minute, time.Second)>DateTime.Now) {
                    reminder.DateTime = new DateTime(this.year, this.month, this.day, time.Hour, time.Minute, time.Second);
                    reminder.ChatId = chatId;
                    reminder.Id = 0;
                    await _reminderService.CreateReminder(reminder);
                    await SendMessage(chatId, $"<b>Xatırlatmanız qeydə alındı. ✅</b> \n{reminder.DateTime.ToString("dd/MM/yyyy HH:mm")} tarixində sizə xatırladacayıq 📅\nİşlərnizdə uğurlar arzu edirik 👍", ParseMode.Html);
                    await SendMenuButton(chatId);
                    ResetUserDialogStep(chatId);
                }
                else
                {
                    await SendMessage(chatId, $"<b>⚠️ Zamanda işinlanma edə bilmirik :(\nKeçmiş bir saatı seçdiniz yeniden seçin</b> ⚠️", ParseMode.Html);
                    await ReminderCreateProcessFour(chatId, day.ToString());
                }
                
            }
            else
            {
                await SendMessage(chatId, $"<b>⚠️ Xatırlatma saatını düzgün daxil edin və ya ləğv edin. ⚠️</b>", ParseMode.Html);
                await ReminderCreateProcessFour(chatId, day.ToString());
            }
                
                
        }
        private int GetUserDialogStep(long chatId)
        {
            if (_userDialogSteps.ContainsKey(chatId))
            {
                return _userDialogSteps[chatId];
            }
            else
            {
                return 0; // Varsayılan olarak 0. adımı dön
            }
        }

        private void UpdateUserDialogStep(long chatId, int step)
        {
            if (_userDialogSteps.ContainsKey(chatId))
            {
                _userDialogSteps[chatId] = step;
            }
            else
            {
                _userDialogSteps.Add(chatId, step);
            }
        }

        private void ResetUserDialogStep(long chatId)
        {
            if (_userDialogSteps.ContainsKey(chatId))
            {
                _userDialogSteps.Remove(chatId);
            }
        }
    }
}
