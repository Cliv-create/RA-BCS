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

    internal class CSTelegramBot
    {
        // TODO: Rename Program into TelegramBotServer / CSTelegramBot
        // TODO: Rename Main function into Start
        // TODO: Move renamed class into it's own file
        // TODO: Get TOKEN from a file. - DONE
        // TODO: ALLOWEDID class for ALLOWED_ID array.
        private static ITelegramBotClient _botClient;
        private static ReceiverOptions _receiverOptions;

        public async Task Main(string[] args = null)
        {
            /*
             * This is RA-BCS (Remote Api-Based Control Server). A "student project" aimed at using API's, listening to incoming messages and launch utilities.
             * "Utilities" in this case - YT-DLP. You can find this project at: https://github.com/yt-dlp/yt-dlp
             * Objective: Launch YT-DLP at specified path with predefined options and URL that was provided throught API messages.
             * Additional objectives (probability of implementation is low): List files from specified folder, and move them to another folder (by specifying files to move as numbers)
             * Started from this guide: https://habr.com/ru/articles/756814/
             * 
             * Directory containing .exe must have "secret.txt" file.
             * It should only have 1 line - bot token.
             * WARNING! DO NOT SHARE BOT TOKEN!
            */
            Console.WriteLine("Starting up!");
            // Token retreival
            string token = "";
            try
            {
                if (!System.IO.File.Exists("secret.txt"))
                {
                    Console.WriteLine("secret.txt not found!");
                    System.IO.File.Create("secret.txt").Dispose(); // if file doesn't exist - create new secret.txt file and immediatly close FileStream (otherwise file will be left open)
                    throw new Exception("File not found. Created an empty file.");
                    // return;
                }
                // TODO: Change this for settings.json later
                token = Convert.ToString(System.IO.File.ReadAllText("secret.txt")); // If file exists - grab all lines (secret.txt should have 1 line only (token))
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("File not found!\n" + ex.ToString());
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine("Directory not found!\n" + ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
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

            using var cts = new CancellationTokenSource();

            // TODO: If implementing multiple async tasks move to this approach:
            // https://learn.microsoft.com/ru-ru/dotnet/standard/parallel-programming/how-to-cancel-a-task-and-its-children
            var receivingTask = Task.Run(() =>
                _botClient.StartReceiving(
                    UpdateHandler,
                    ErrorHandler,
                    _receiverOptions,
                    cts.Token
                ),
                cts.Token
            );

            // _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Launching bot

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

            // Commented-out because CancellationToken was implemented. For removal.
            // await Task.Delay(-1); // Infinite delay, so that the bot is always up.
        }

        /*
        public static async Task<bool> SendTextMessage()
        {
            await botClient.SendMessage(
                chat.Id,
                text:   "Привет!\n" +
                        "Это проект RA-BCS для удалённого управления компьютером.",
                protectContent: true,
                replyParameters: message.MessageId
            );
            return false;
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
                                                     // chat.Id; // Allowed ID check for later

                            switch (message.Type)
                            {
                                #region Text MessageType
                                case MessageType.Text:
                                    {
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

                                        if (message.Text == "[ЗАРЕЗЕРВИРОВАННО]")
                                        {
                                            await botClient.SendMessage(
                                                chat.Id,
                                                text: "Пока не готово!",
                                                protectContent: true,
                                                replyParameters: message.MessageId
                                            );
                                            // return;
                                        }
                                        #endregion


                                        #region Commands IF level
                                        if (message.Text == "/start")
                                        {
                                            await botClient.SendMessage(
                                                chat.Id,
                                                text:   "Выбери клавиатуру:\n" +
                                                        "/inline\n" +
                                                        "/reply\n",
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
                                            InlineKeyboardButton.WithCallbackData("А это просто кнопка", "button1"),
                                        },
                                        new InlineKeyboardButton[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Тут еще одна", "button2"),
                                            InlineKeyboardButton.WithCallbackData("И здесь", "button3"),
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
                                                new KeyboardButton("[ЗАРЕЗЕРВИРОВАННО]")
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

                                        // Out of text checks
                                        // MessageType.Text return
                                        return;
                                    }
                                #endregion
                                // Default to show difference in message types
                                default:
                                    {
                                        await botClient.SendMessage(
                                            chat.Id,
                                            text: "Используй только текст!",
                                            protectContent: true,
                                            replyParameters: message.MessageId
                                        );
                                        return;
                                    }
                                    // MessageType.Text level
                            }
                            // message.Type return (unreachable code)
                            return;

                            // Old code:
                            /*
                            await botClient.SendMessage(
                                chat.Id,
                                message.Text, // Sending text that user sent to us
                                protectContent: true,
                                replyParameters: message.MessageId // Reply to sent message or not
                            );

                            return;
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

                            // Добавляем блок switch для проверки кнопок
                            switch (callbackQuery.Data)
                            {
                                // Data - это придуманный нами id кнопки, мы его указывали в параметре
                                // callbackData при создании кнопок. У меня это button1, button2 и button3

                                case "button1":
                                    {
                                        // В этом типе клавиатуры обязательно нужно использовать следующий метод
                                        await botClient.AnswerCallbackQuery(callbackQuery.Id);
                                        // Для того, чтобы отправить телеграмму запрос, что мы нажали на кнопку

                                        await botClient.SendMessage(
                                            chat.Id,
                                            text: $"Вы нажали на {callbackQuery.Data}",
                                            protectContent: true
                                        );
                                        return;
                                    }

                                case "button2":
                                    {
                                        // А здесь мы добавляем наш сообственный текст, который заменит слово "загрузка", когда мы нажмем на кнопку
                                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "Тут может быть ваш текст!");

                                        await botClient.SendMessage(
                                            chat.Id,
                                            text: $"Вы нажали на {callbackQuery.Data}",
                                            protectContent: true
                                        );
                                        return;
                                    }

                                case "button3":
                                    {
                                        // А тут мы добавили еще showAlert, чтобы отобразить пользователю полноценное окно
                                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "А это полноэкранный текст!", showAlert: true);

                                        await botClient.SendMessage(
                                            chat.Id,
                                            text: $"Вы нажали на {callbackQuery.Data}",
                                            protectContent: true
                                        );
                                        return;
                                    }
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
