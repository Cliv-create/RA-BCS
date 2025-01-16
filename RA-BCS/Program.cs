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
            /*
             * This is RA-BCS (Remote Api-Based Control Server). A "student project" aimed at using API's, listening to incoming messages and launch utilities.
             * "Utilities" in this case - YT-DLP. You can find this project at: https://github.com/yt-dlp/yt-dlp
             * Objective: Launch YT-DLP at specified path with predefined options and URL that was provided throught API messages.
             * Additional objectives (probability of implementation is low): List files from specified folder, and move them to another folder (by specifying files to move as numbers)
             * Started from this guide: https://habr.com/ru/articles/756814/
             * 
             * Directory containing .exe must have "config.json" file.
             * To generate config.json use ConfigManager.GenerateInitialConfig() method.
             * WARNING! DO NOT SHARE BOT TOKEN!
            */
            try
            {
                ConfigManager.LoadConfig();
                // ConfigManager.GenerateInitialConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            // Telegram Bot instance
            CSTelegramBot telegram_bot = new CSTelegramBot();
            await telegram_bot.Main();
        }
    }
}