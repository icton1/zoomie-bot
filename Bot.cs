using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;

namespace GimmeTheZoomBot
{
    public class Bot
    {
        public ITelegramBotClient _botClient;

        public Bot(string token)
        {
            _botClient = new TelegramBotClient(token);
            _botClient.OnMessage += StartCommand;
            _botClient.OnMessage += GetGmailMessagesCommand;
            _botClient.OnMessage += GetTodayCommand;
            _botClient.OnMessage += GetTomorrowCommand;
            _botClient.OnCallbackQuery += AuthCommand;
        }

        public void Start()
        {
            _botClient.StartReceiving();
        }

        private async void GetGmailMessagesCommand(object sender, MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.Text)
            {
                if (e.Message.Text.Contains(@"/get_last"))
                {
                    List<string> messages = new List<string>();
                    string[] param = e.Message.Text.Split();
                    int number;

                    if(param.Length > 1 && int.TryParse(param[1], out number))
                    {
                         messages = GmailServiceWorker.GetGmailMessages(number);
                    }
                    else
                    {
                         messages = GmailServiceWorker.GetGmailMessages();
                    }
                    
                    
                    foreach(var message in messages)
                    {
                        await _botClient.SendTextMessageAsync(e.Message.Chat.Id, message);
                    }
                    
                }
            }
        }

        private async void AuthCommand(object sender, CallbackQueryEventArgs e)
        {
            var message = e.CallbackQuery;
            if (message.Data == "Auth")
            {
                GmailServiceWorker.Init(message.Message.Chat.Id);
                GmailServiceWorker.GetService();

                string messageToUser = $"Вы успешно прошли авторизацию, как {GmailServiceWorker.GetGmailName(message.Message.Chat.Id)}";

                await _botClient.SendTextMessageAsync(message.Message.Chat.Id, messageToUser);
            }
            else if (message.Data == "ReAuth")
            {
                GmailServiceWorker.ReInit(message.Message.Chat.Id);
                GmailServiceWorker.GetService();
            }
        }



        private async void StartCommand(object sender, MessageEventArgs e)
        {
            //e.Message.Chat.Type == ChatType.Group
            if (e.Message.Type == MessageType.Text)
            {
                if (e.Message.Text == @"/start")
                {
                    InlineKeyboardMarkup inlineKeyboard;
                    string message;

                    //if (!GmailServiceWorker.IsAuth(e.Message.Chat.Id))
                    {
                        inlineKeyboard = new InlineKeyboardMarkup(
                            new[] { InlineKeyboardButton.WithCallbackData("Авторизируйся здесь!", "Auth") }
                        );

                        message = "Привет! Чтобы получать уведомления о парах, необходимо подключить почту!";
                    }
                    //else
                    //{
                    //    inlineKeyboard = new InlineKeyboardMarkup(
                    //        new[] { InlineKeyboardButton.WithCallbackData("Авторизовать другую почту!", "ReAuth") }
                    //    );

                    //    message = $"Ты уже авторизован как {GmailServiceWorker.GetGmailName(e.Message.Chat.Id)}";
                    //}

                    await _botClient.SendTextMessageAsync(e.Message.Chat.Id, message, replyMarkup: inlineKeyboard);
                }
            }
        }

        private async void GetTodayCommand(object sender, MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.Text)
            {
                if (e.Message.Text == @"/get_tomorrow")
                {
                    var now = DateTime.Now;
                    now = now.AddDays(1);
                    
                    List<string> messages = GmailServiceWorker.GetGmailMessages(now);

                    if(messages != null && messages.Count > 0)
                    {
                        foreach (var message in messages)
                        {
                            await _botClient.SendTextMessageAsync(e.Message.Chat.Id, message);
                        }
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Завтра пар нет! Можно отдохнуть!");
                    }
                }
            }
        }

        private async void GetTomorrowCommand(object sender, MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.Text)
            {
                if (e.Message.Text == @"/get_today")
                {
                    var now = DateTime.Now;

                    List<string> messages = GmailServiceWorker.GetGmailMessages(now);

                    if (messages != null && messages.Count > 0)
                    {
                        foreach (var message in messages)
                        {
                            await _botClient.SendTextMessageAsync(e.Message.Chat.Id, message);
                        }
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Сегодня пар нет! Можно отдохнуть!");
                    }
                }
            }
        }
    }
}
