using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.bot;

public partial class UpdateHandlers
{
    private static async Task OnMessageReceived(ITelegramBotClient botClient, Message message)
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(message.From.LanguageCode??"ru");

        //Console.WriteLine($"Receive message type: {message.Type}");
        if (message.Type != MessageType.Text)
            return;
        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        InlineKeyboardMarkup inlineKeyboard = new(new[] {
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Pills, "pills"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Transport, "transport"), }, });
        var header = Resources.Translation.MainMenu;
        if (message.Text == "updb")
        {
            if (await Services.Gsheet.SyncPills(_db) == 0)
            {
                header += "\n" + Resources.Translation.DBUpdateFinished;
            }
            else
            {
                header += "\n" + Resources.Translation.DBUpdateFailed;
            }
        }
        await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: header, replyMarkup: inlineKeyboard);
    }
}
