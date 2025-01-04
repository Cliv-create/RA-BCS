using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Transactions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Telegram.Bot.TelegramBotClient;

namespace RA_BCS
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Telegram Bot instance
            CSTelegramBot telegram_bot = new CSTelegramBot();
            await telegram_bot.Main();
        }
    }
}