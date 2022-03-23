using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.bot;

public class UpdateHandlers
{
    public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message            => OnMessageReceived(botClient, update.Message!),
            UpdateType.EditedMessage      => OnMessageReceived(botClient, update.EditedMessage!),
            UpdateType.CallbackQuery      => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
            UpdateType.InlineQuery        => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
            UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
            _                             => UnknownUpdateHandlerAsync(botClient, update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(botClient, exception, cancellationToken);
        }
    }

    private static async Task OnMessageReceived(ITelegramBotClient botClient, Message message)
    {
        Console.WriteLine($"Receive message type: {message.Type}");
        if (message.Type != MessageType.Text)
            return;

        var action = message.Text!.Split(' ')[0] switch
        {
            "/inline" => AnswerHandlers.SendInlineKeyboard(botClient, message),
            "/keyboard" => AnswerHandlers.SendReplyKeyboard(botClient, message),
            "/remove" => AnswerHandlers.RemoveKeyboard(botClient, message),
            "/photo" => AnswerHandlers.SendFile(botClient, message),
            "/request" => AnswerHandlers.RequestContactAndLocation(botClient, message),
            _ => AnswerHandlers.Usage(botClient, message)
        };
        Message sentMessage = await action;
        Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");
    }

    // Process Inline Keyboard callback data
    private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        await botClient.AnswerCallbackQueryAsync( callbackQueryId: callbackQuery.Id, text: $"Received {callbackQuery.Data}");
        await botClient.SendTextMessageAsync( chatId: callbackQuery.Message.Chat.Id, text: $"Received {callbackQuery.Data}");
    }

    private static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery)
    {
        Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");
        InlineQueryResult[] results = { new InlineQueryResultArticle( id: "3", title: "TgBots", inputMessageContent: new InputTextMessageContent( "hello" ))};
        await botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id, results: results, isPersonal: true, cacheTime: 0);
    }

    private static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult)
    {
        Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
        return Task.CompletedTask;
    }

    private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        Console.WriteLine($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }
}
