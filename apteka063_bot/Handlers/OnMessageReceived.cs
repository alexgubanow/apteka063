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
        string locale = message.From.LanguageCode ?? "ru";
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(locale);

        //Console.WriteLine($"Receive message type: {message.Type}");
        if (message.Type != MessageType.Text)
            return;
        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        var header = Resources.Translation.MainMenu;
        if (message.Text == "updb")
        {
            if (await Services.Gsheet.SyncPillsAsync(_db) == 0)
            {
                header += "\n" + Resources.Translation.DBUpdateFinished;
            }
            else
            {
                header += "\n" + Resources.Translation.DBUpdateFailed;
            }
        }
        await ShowMainMenu(botClient, locale, message.Chat.Id, header);
    }
    private static async Task ShowMainMenu(ITelegramBotClient botClient, string locale, long chatId, string headerText, int? messageId = null)
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(locale);

        InlineKeyboardMarkup inlineKeyboard = new(new[] {
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Pills, "pills"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Transport, "transport"), }, });
        if (messageId != null)
        {
            await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId ?? 0, text: headerText, replyMarkup: inlineKeyboard);
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId: chatId, text: headerText, replyMarkup: inlineKeyboard);
        }
    }
}
