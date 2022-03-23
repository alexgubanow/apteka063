using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.bot;

public class AnswerHandlers
{
    public static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    },
            });

        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Choose", replyMarkup: inlineKeyboard);
    }

    public static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message)
    {
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] {
            new KeyboardButton[] { "1.1", "1.2" },
            new KeyboardButton[] { "2.1", "2.2" },})
        { ResizeKeyboard = true };
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Choose", replyMarkup: replyKeyboardMarkup);
    }

    public static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message)
    {
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Removing keyboard", replyMarkup: new ReplyKeyboardRemove());
    }
    public static async Task<Message> SendFile(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

        const string filePath = @"Files/tux.png";
        using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

        return await botClient.SendPhotoAsync(chatId: message.Chat.Id, photo: new InputOnlineFile(fileStream, fileName), caption: "Nice Picture");
    }
    public static async Task<Message> RequestContactAndLocation(ITelegramBotClient botClient, Message message)
    {
        ReplyKeyboardMarkup RequestReplyKeyboard = new(
            new[]
            {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
            });

        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Who or Where are you?", replyMarkup: RequestReplyKeyboard);
    }

    public static async Task<Message> Usage(ITelegramBotClient botClient, Message message)
    {
        const string usage = "Usage:\n" +
                             "/inline   - send inline keyboard\n" +
                             "/keyboard - send custom keyboard\n" +
                             "/remove   - remove custom keyboard\n" +
                             "/photo    - send a photo\n" +
                             "/request  - request location or contact";

        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: usage, replyMarkup: new ReplyKeyboardRemove());
    }

}
