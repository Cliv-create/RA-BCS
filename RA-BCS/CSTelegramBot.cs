using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Telegram.Bot.TelegramBotClient;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using System.Globalization;

namespace RA_BCS
{
    // TODO: Use this class as embedded into messengerClass later on
    /*
    class ALLOWEDIDLIST
    {
        static readonly long[] allowed_user_id_list;
        static readonly long[] allowed_channel_id_list;
        public long[] AllowedChannelIdList { get; }

        
        public ALLOWEDIDLIST(string file_path)
        {
            // Naming convention:
            // JSON: allowed_id_list array
            // TXT: Line that starts with "ALLOWEDIDLIST: " at the beginnning.
            // TODO: Read file lines, find "ALLOWEDIDLIST:" at the beginning, everything else goes into array as id
            AllowedChannelIdList[0] = 12321312; // TEMP
        }
    }
    */

    internal partial class CSTelegramBot
    {
        // TODO: Rename Main function into Start
        // TODO: ALLOWEDID class for ALLOWED_ID array.

        private ITelegramBotClient _botClient;
        private ReceiverOptions _receiverOptions;
        
        private readonly string token = "";

        // ---

        // Match YoutubeID pattern
        // Link to the pattern: https://regex101.com/library/OY96XI
        // WARNING: Don't forget to switch FLAVOR setting to .NET 7.0 (C#) in order to debug and test pattern
        // YoutubeIDPattern
        [GeneratedRegex(@"(?:https?:)?(?:\/\/)?(?:[0-9A-Z-]+\.)?(?:youtu\.be\/|youtube(?:-nocookie)?\.com\S*?[^\w\s-])([\w-]{11})(?=[^\w-]|$)(?![?=&+%\w.-]*(?:['""][^<>]*>|<\/a>))[?=&+%\w.-]*", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex YoutubeVideoIDRegex();
        
        // Return match after /link command
        // (?<=\/link ).*
        [GeneratedRegex(@"(?<=\/link ).*", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MatchAfterLinkCommand();

        // Return match after /move command.
        [GeneratedRegex(@"(?<=\/move ).*", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex MatchAfterMoveCommand();

        // Return match after /move command (matches "/move all" and "/move 1 2 3 ... 9" (in any order))
        // ^/move\s+(?:(?<all>all)|(?<numbers>(?:\d+\s*)+))$
        [GeneratedRegex(@"^/move\s+(?:(?<all>all)|(?<numbers>(?:\d+\s*)+))$", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex FindMatchAfterMoveCommand();

        // ---

        /// <summary>
        /// CSTelegramBot constructor with parameters. Required bot token. Token get's checked if it's null or empty.
        /// </summary>
        /// <param name="token">Bot token to create bot instance.</param>
        public CSTelegramBot(string token)
        {
            Console.WriteLine("Starting up!");
            // Token retreival
            this.token = token;
            if (token == "" || token == null)
            {
                Console.WriteLine("Empty or null token detected!\nExiting...");
                System.Environment.ExitCode = -1;
            }
            _botClient       = new TelegramBotClient(token);
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
                {
                    UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
                    UpdateType.CallbackQuery, // Inline кнопки
                },
                DropPendingUpdates = true,
            };
        }
        
        /// <summary>
        /// CSTelegramBot constructor without parameters. Requires config.json file in directory with bot token.
        /// </summary>
        public CSTelegramBot()
        {
            Console.WriteLine("Building bot instance!");

            // Token retreival
            token = ConfigManager.Get("telegram_token");

            if (token == "" || token == null)
            {
                Console.WriteLine("Empty or null token detected!\nExiting...");
                System.Environment.ExitCode = -1;
            }

            _botClient       = new TelegramBotClient(token);
            _receiverOptions = new ReceiverOptions
            {                           
                AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
                {
                    UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
                    UpdateType.CallbackQuery, // Inline кнопки
                },
                // ThrowPendingUpdates = true,
                DropPendingUpdates = true,
            };

            Console.WriteLine("Built bot instance successfully!");
        }

        public async Task Main(string[] args = null)
        {
            /*
             * This is RA-BCS (Remote Api-Based Control Server). A "student project" aimed at using API's, listening to incoming messages and launch utilities.
             * "Utilities" in this case - YT-DLP. You can find this project at: https://github.com/yt-dlp/yt-dlp
             * Objective: Launch YT-DLP at specified path with predefined options and URL that was provided throught API messages.
             * Additional objectives (probability of implementation is low): List files from specified folder, and move them to another folder (by specifying files to move as numbers)
             * Started from this guide: https://habr.com/ru/articles/756814/
             * 
             * Directory containing .exe must have "config.json" file.
             * It should have telegram_token set to your telegram bot token.
             * WARNING! DO NOT SHARE BOT TOKEN!
            */
            
            // TODO: Change CancellationToken handler to Task.Delay(-1, cancellationToken: cts.Token.)
            // Link for the change: https://habr.com/ru/articles/657583/comments/#comment_24205299
            using var cts = new CancellationTokenSource();

            // TODO: If implementing multiple async tasks move to this approach:
            // https://learn.microsoft.com/ru-ru/dotnet/standard/parallel-programming/how-to-cancel-a-task-and-its-children
            // Launching bot
            Console.WriteLine("Launching bot!");
            var receivingTask = Task.Run(() =>
                _botClient.StartReceiving(
                    UpdateHandler,
                    ErrorHandler,
                    _receiverOptions,
                    cts.Token
                ),
                cts.Token
            );

            var me = await _botClient.GetMe();
            Console.WriteLine($"{me.FirstName} launched!");

            Console.WriteLine("Enter \"c\" or \"C\" to exit...");
            char ch = Console.ReadKey().KeyChar;
            if (ch == 'c' || ch == 'C')
            {
                cts.Cancel();
                Console.WriteLine("\nMain: Task cancellation requested.");
            }

            try
            {
                await receivingTask;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"\nMain: {nameof(OperationCanceledException)} thrown\n");
            }
            finally
            {
                cts.Dispose();
            }
        }

        /* SendTextMessage (unused)
        public static async Task<bool> SendTextMessage(string text)
        {
            await botClient.SendMessage(
                chat.Id,
                text:   $"{text}",
                protectContent: false,
                replyParameters: message.MessageId
            );
            return true;
        }
        */

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Обязательно ставим блок try-catch, чтобы наш бот не "падал" в случае каких-либо ошибок
            try
            {
                // CancellationToken check (Check https://learn.microsoft.com/ru-ru/dotnet/standard/parallel-programming/how-to-cancel-a-task-and-its-children for additional information)
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Task {0} was cancelled before it got started.", Task.CurrentId);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                // Сразу же ставим конструкцию switch, чтобы обрабатывать приходящие Update
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            var message = update.Message;
                            var user    = message.From;
                            Console.WriteLine($"{user.FirstName} ({user.Id}) wrote message: {message.Text}");
                            var chat = message.Chat; // All info about chat
                                                     // chat.Id; - allows ID check for later

                            switch (message.Type)
                            {
                                #region MessageType - Text
                                case MessageType.Text:
                                    {
                                        #region Commands level
                                        // Link check
                                        if (MatchAfterLinkCommand().IsMatch(message.Text))
                                        {
                                            if (YoutubeVideoIDRegex().IsMatch(message.Text))
                                            {
                                                var sentMessage = await botClient.SendMessage(
                                                    // TODO: Delete
                                                    // chatId: message.Chat.Id,
                                                    chat.Id,
                                                    text:   "Starting download...",
                                                    protectContent: false, // Change to true if needed.
                                                    replyParameters: message.MessageId
                                                );

                                                // YTDLP instance for download.
                                                YTDLP ytdlp = new YTDLP();

                                                // 3500 symbols to minimize memory re-allocation. Change this, if the average amount of symbols in the output changed.
                                                StringBuilder cumulative_output = new StringBuilder(3500);
                                                
                                                var progress = new Progress<string>(async (output) =>
                                                {
                                                    try
                                                    {
                                                    // Appending new output to the previous
                                                    cumulative_output.AppendLine(output);

                                                    // Updating message (cumulative output)
                                                    await botClient.EditMessageText(
                                                        chatId: sentMessage.Chat.Id,
                                                        messageId: sentMessage.MessageId,
                                                        text:   $"Download progress:\n{cumulative_output}"
                                                    );

                                                    /* Replace cumulative output to get only last update messages.
                                                    // Updating message
                                                    await botClient.EditMessageText(
                                                        chatId: sentMessage.Chat.Id,
                                                        messageId: sentMessage.MessageId,
                                                        text:   $"Download progress:\n{output}"
                                                    );
                                                    */
                                                    }
                                                    catch (ArgumentOutOfRangeException ex)
                                                    {
                                                        Console.WriteLine($"ArgumentOutOfRangeException! Argument value: {ex.ActualValue}.\nToString: {ex.ToString}");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Console.WriteLine($"Caught exception! Message: {ex.Message}.\nToString: {ex.ToString}");

                                                        // Sending error message to the user.
                                                        await botClient.EditMessageText(
                                                            chatId: sentMessage.Chat.Id,
                                                            messageId: sentMessage.MessageId,
                                                            text:   $"Error occured!:\n{ex.ToString}"
                                                        );
                                                    }
                                                });
                                                
                                                // Starting download, also tracking IProgress<string> changes.
                                                await ytdlp.StartDownload(YoutubeVideoIDRegex().Match(message.Text).ToString(), progress);

                                                await botClient.SendMessage(
                                                    chat.Id,
                                                    text:   "Download completed.",
                                                    protectContent: true,
                                                    replyParameters: message.MessageId
                                                );
                                            }
                                            else
                                            {
                                                await botClient.SendMessage(
                                                    chat.Id,
                                                    text:   "Link not found. Returning...",
                                                    protectContent: true,
                                                    replyParameters: message.MessageId
                                                );
                                            }
                                            return;
                                        }
                                        
                                        // Move check
                                        if (MatchAfterMoveCommand().IsMatch(message.Text))
                                        {
                                            Console.WriteLine($"Matched after /move command with following text: {message.Text}");
                                            Match match = FindMatchAfterMoveCommand().Match(message.Text);
                                            
                                            if (match.Success)
                                            {
                                                var sentMessage = await botClient.SendMessage(
                                                    chat.Id,
                                                    text:   "Starting file move...",
                                                    protectContent: false, // Change to true if needed.
                                                    replyParameters: message.MessageId
                                                );

                                                Console.WriteLine("Matched pattern successfully!");
                                                try
                                                {
                                                    if (match.Groups["all"].Success)
                                                    {
                                                        Console.WriteLine(@"Matched 'all' group. Starting file move.");

                                                        await FileHandler.MoveFilesToDirectory(
                                                            ConfigManager.Get("yt-dlp_download_path"),
                                                            ConfigManager.Get("downloaded_files_destination"),
                                                            move_all: true
                                                        );
                                                    }
                                                    else if (match.Groups["numbers"].Success)
                                                    {
                                                        Console.WriteLine("Matched 'numbers' group. Starting int[] array parse.");

                                                        string numbers_string = match.Groups["numbers"].Value.Trim();
                                                        string[] number_strings = numbers_string.Split(new[] { ",", ", " }, StringSplitOptions.RemoveEmptyEntries);

                                                        int[] numbers = Array.ConvertAll(number_strings, int.Parse);
                                                        Console.WriteLine($"Finished int[] array parse. Array: {numbers}");

                                                        await FileHandler.MoveFilesToDirectory(
                                                            ConfigManager.Get("yt-dlp_download_path"),
                                                            ConfigManager.Get("downloaded_files_destination"),
                                                            files_to_move: numbers
                                                        );
                                                    }
                                                    await botClient.EditMessageText(
                                                        chatId: sentMessage.Chat.Id,
                                                        messageId: sentMessage.MessageId,
                                                        text:   "Moved files succeffsully!"
                                                    );
                                                }
                                                catch (ArgumentNullException ex)
                                                {
                                                    Console.WriteLine($"Catched ArgumentNullException exception! Maybe input was null? Message: {ex.Message}\nTo string: {ex.ToString}");
                                                    
                                                    // Sending error message to the user.
                                                    await botClient.EditMessageText(
                                                        chatId: sentMessage.Chat.Id,
                                                        messageId: sentMessage.MessageId,
                                                        text:   $"Error occured! (null occured):\n{ex.ToString}"
                                                    );
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"Catched an exception! Message: {ex.Message}\nTo string: {ex.ToString}");

                                                    // Sending error message to the user.
                                                    await botClient.EditMessageText(
                                                        chatId: sentMessage.Chat.Id,
                                                        messageId: sentMessage.MessageId,
                                                        text:   $"Error occured!:\n{ex.ToString}"
                                                    );
                                                }
                                            }
                                            else
                                            {
                                                await botClient.SendMessage(
                                                    chat.Id,
                                                    text:   "Move command failed to parse!. Returning...",
                                                    protectContent: true,
                                                    replyParameters: message.MessageId
                                                );
                                            }
                                            return;
                                        }

                                        #endregion

                                        #region Reply-keyboard IF level
                                        if (message.Text == "Привет! Ты кто?")
                                        {
                                            await botClient.SendMessage(
                                                chat.Id,
                                                text:   "Привет!\n" +
                                                        "Это проект RA-BCS для удалённого управления компьютером.",
                                                protectContent: true,
                                                replyParameters: message.MessageId
                                            );

                                            return;
                                        }

                                        if (message.Text == "Пока!")
                                        {
                                            await botClient.SendMessage(
                                                chat.Id,
                                                text: "Пока!\n",
                                                protectContent: true,
                                                replyParameters: message.MessageId
                                            );

                                            return;
                                        }

                                        if (message.Text == "Выключись!")
                                        {
                                            await botClient.SendMessage(
                                                chat.Id,
                                                text: "Пока!",
                                                protectContent: true,
                                                replyParameters: message.MessageId
                                            );
                                            System.Environment.Exit(0);
                                            // return;
                                        }

                                        if (message.Text == "Create inline keyboard")
                                        {
                                            await botClient.SendMessage(
                                                chat.Id,
                                                text: "Not implemented!",
                                                protectContent: true,
                                                replyParameters: message.MessageId
                                            );
                                            return;
                                        }
                                        #endregion

                                        #region Commands IF level
                                        if (message.Text == "/start")
                                        {
                                            await botClient.SendMessage(
                                                chat.Id,
                                                text:   "Выбери клавиатуру:\n" +
                                                        "/inline (commands)\n" +
                                                        "/reply\n" +
                                                        "Commands:\n" +
                                                        "/move\n" +
                                                        "Usage: <code>/move all</code> to move all files or <code>/move [numbers of files to move]</code> (use Show files in Download location command to get needed numbers)\n",
                                                ParseMode.Html,
                                                protectContent: true,
                                                replyParameters: message.MessageId
                                            );

                                            return;
                                        }

                                        if (message.Text == "/inline")
                                        {
                                            // Creating our keyboard
                                            var inlineKeyboard = new InlineKeyboardMarkup(
                                            new List<InlineKeyboardButton[]>() // здесь создаем лист (массив), который содержит в себе массив из класса кнопок
                                            {
                                                // Каждый новый массив - это дополнительные строки,
                                                // а каждая дополнительная строка (кнопка) в массиве - это добавление ряда

                                                new InlineKeyboardButton[] // тут создаем массив кнопок
                                                {
                                                    InlineKeyboardButton.WithUrl("GitHub", "https://habr.com/"),
                                                    InlineKeyboardButton.WithCallbackData("Command list", "button1"),
                                                },
                                                new InlineKeyboardButton[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Show YTDLP path", "button2"),
                                                    InlineKeyboardButton.WithCallbackData("Show files in Download location", "button3"),
                                                },
                                                new InlineKeyboardButton[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Show downloaded files move path", "button4"),
                                                    InlineKeyboardButton.WithCallbackData("[TEMP]", "button5"),
                                                },
                                            });

                                            await botClient.SendMessage(
                                                chat.Id,
                                                text: "Это inline клавиатура!",
                                                protectContent: true,
                                                replyParameters: message.MessageId,
                                                replyMarkup: inlineKeyboard
                                            ); // Все клавиатуры передаются в параметр replyMarkup

                                            return;
                                        }

                                        if (message.Text == "/reply")
                                        {
                                            // Тут все аналогично Inline клавиатуре, только меняются классы
                                            // НО! Тут потребуется дополнительно указать один параметр, чтобы
                                            // клавиатура выглядела нормально, а не как абы что
                                            var replyKeyboard = new ReplyKeyboardMarkup(
                                                new List<KeyboardButton[]>()
                                                {
                                                    new KeyboardButton[]
                                                    {
                                                        new KeyboardButton("Привет! Ты кто?"),
                                                        new KeyboardButton("Пока!"),
                                                    },
                                                    new KeyboardButton[]
                                                    {
                                                        new KeyboardButton("Выключись!")
                                                    },
                                                    new KeyboardButton[]
                                                    {
                                                        new KeyboardButton("Create inline keyboard")
                                                    }
                                                }
                                            )
                                            {
                                                // автоматическое изменение размера клавиатуры, если не стоит true,
                                                // тогда клавиатура растягивается чуть ли не до луны,
                                                // проверить можете сами
                                                ResizeKeyboard = true,
                                            };

                                            await botClient.SendMessage(
                                                    chat.Id,
                                                    text: "Это reply клавиатура!",
                                                    protectContent: true,
                                                    replyParameters: message.MessageId,
                                                    replyMarkup: replyKeyboard
                                            ); // опять передаем клавиатуру в параметр replyMarkup

                                            return;
                                        }
                                        #endregion

                                        // MessageType.Text return
                                        return;
                                    }
                                #endregion
                                // Default to show difference in message types
                                default:
                                {
                                    await botClient.SendMessage(
                                        chat.Id,
                                        text: "Command not recognized!",
                                        protectContent: true,
                                        replyParameters: message.MessageId
                                    );
                                    return;
                                }
                                // MessageType.Text level
                            }
                            /* Send message example:
                            await botClient.SendMessage(
                                chat.Id,
                                text: "", // Sending text
                                protectContent: true, // Will message be copy-able, or not
                                replyParameters: message.MessageId // Reply to sent message
                            );
                            */
                        }

                    #region UpdateType CallbackQuery (inline response handle)
                    case UpdateType.CallbackQuery:
                    {
                        // Переменная, которая будет содержать в себе всю информацию о кнопке, которую нажали
                        var callbackQuery = update.CallbackQuery;

                        // Аналогично и с Message мы можем получить информацию о чате, о пользователе и т.д.
                        var user = callbackQuery.From;

                        // Выводим на экран нажатие кнопки
                        Console.WriteLine($"{user.FirstName} ({user.Id}) pressed button: {callbackQuery.Data}");

                        // Вот тут нужно уже быть немножко внимательным и не путаться!
                        // Мы пишем не callbackQuery.Chat , а callbackQuery.Message.Chat , так как
                        // кнопка привязана к сообщению, то мы берем информацию от сообщения.
                        var chat = callbackQuery.Message.Chat;

                        // Adding switch block for checking buttons
                        switch (callbackQuery.Data)
                        {
                            // Data - our custom button ID that we specified in parameter callbackData when creating buttons: InlineKeyboardButton.WithCallbackData("Command list", "button1")

                            case "button1":
                            {
                                // In this type of keyboard this method is mandatory.
                                // In order to send Telegram request that user pressed a button
                                await botClient.AnswerCallbackQuery(callbackQuery.Id);

                                // TODO: Remove Debug section.
                                await botClient.SendMessage(
                                    chat.Id,
                                    text: $"Commands:\n" +
                                            "/start - Start menu\n" +
                                            "/inline - Info menu\n" +
                                            "/reply - Unused\n" +
                                            "Debug:\n" + 
                                            $"Button pressed: {callbackQuery.Data}",
                                    protectContent: false // TODO: Change this if needed.
                                );
                                return;
                            }

                            case "button2":
                            {
                                // А здесь мы добавляем наш сообственный текст, который заменит слово "загрузка", когда мы нажмем на кнопку
                                // Adding string parameter here makes temporary message appear at the top of chat window.
                                // Example use: Displaying error message to the user.

                                // TODO: Remove text parameter.
                                await botClient.AnswerCallbackQuery(callbackQuery.Id, "Showed YTDLP path!");
                                
                                // TODO: Remove Debug section.
                                await botClient.SendMessage(
                                    chat.Id,
                                    text:   "YTDLP path:\n" +
                                            $"<code>{YTDLP.Path}</code>\n" +
                                            "Debug:\n" + 
                                            $"Button pressed: {callbackQuery.Data}",
                                    ParseMode.Html,
                                    protectContent: true
                                );
                                return;
                            }

                            case "button3":
                            {
                                // In this type of keyboard this method is mandatory.
                                // In order to send Telegram request that user pressed a button
                                await botClient.AnswerCallbackQuery(callbackQuery.Id);
                                try
                                {
                                // TODO: Change the code to better approach.
                                StringBuilder files_list = new StringBuilder(1500);
                                string[] directory_files = await FileHandler.GetFilesFromDirectory(YTDLP.DownloadPath);
                                
                                for (int i = 0; i < directory_files.Length; i++)
                                {
                                    files_list.Append($"{i} - ");
                                    files_list.AppendLine(directory_files[i]);
                                }
                                // TODO: Remove Debug section.
                                await botClient.SendMessage(
                                    chat.Id,
                                    text:   $"Files list: \n" +
                                            $"{files_list.ToString()}" +
                                            "Debug:\n" + 
                                            $"Button pressed: {callbackQuery.Data}",
                                    protectContent: false // TODO: Change this if needed.
                                );
                                }
                                catch (DirectoryNotFoundException ex)
                                {
                                    Console.WriteLine($"Directory not found!\n"
                                                      + $"{ex.ToString()}");
                                }
                                return;
                            }

                            case "button4":
                            {
                                // TODO: Remove text parameter.
                                await botClient.AnswerCallbackQuery(callbackQuery.Id, "Showed downloaded file destination path!");
                                
                                // TODO: Remove Debug section.
                                await botClient.SendMessage(
                                    chat.Id,
                                    text:   "Destination file path for downloaded files:\n" +
                                            $"<code>{ConfigManager.Get("downloaded_files_destination")}</code>\n" +
                                            "Debug:\n" + 
                                            $"Button pressed: {callbackQuery.Data}",
                                    ParseMode.Html,
                                    protectContent: true
                                );
                                return;
                            }

                            /*
                            case "button4":
                            {
                                // Setting showAlert to true makes a full window appear for the user.
                                await botClient.AnswerCallbackQuery(callbackQuery.Id, "Full screen text!", showAlert: true);

                                await botClient.SendMessage(
                                    chat.Id,
                                    text: $"You pressed on: {callbackQuery.Data}",
                                    protectContent: true
                                );

                                return;
                            }
                            */
                        }
                        return;
                    }
                    #endregion
                    // switch(update.Type) level
                }
            }
            catch (OperationCanceledException ocex)
            {
                Console.WriteLine("\nOperation was canceled");
                Console.WriteLine($"Application/Object that caused: {ocex.Source}\nDescription: {ocex.Message}\nInstance that caused exception: {ocex.InnerException}\nHRESULT:{ocex.HResult}\nex string: {ocex.ToString}");
                // returning from method automatically (cancellation was requested)
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

    }
}
