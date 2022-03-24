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
        //Console.WriteLine($"Receive message type: {message.Type}");
        if (message.Type != MessageType.Text)
            return;
        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        InlineKeyboardMarkup inlineKeyboard = new(new[] {
            new [] { InlineKeyboardButton.WithCallbackData("Pills", "pills"), },
            new [] { InlineKeyboardButton.WithCallbackData("Transport", "transport"), }, });

        await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Main menu:", replyMarkup: inlineKeyboard);
    }
}
