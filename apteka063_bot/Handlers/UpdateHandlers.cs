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
    private readonly static dbc.Apteka063Context _db = new();
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
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(GetLangaugeCodeFromUpdate(update));

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
            UpdateType.CallbackQuery      => OnQueryReceived(botClient, update.CallbackQuery!),
            //UpdateType.InlineQuery        => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
            //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
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

    private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        //Console.WriteLine($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }

    private static string GetLangaugeCodeFromUpdate(Update update)
    {
        string locale = "ru";
        bool localeFound = false;
        try
        {
            locale = update.CallbackQuery.From.LanguageCode;
            localeFound = true;
        }
        catch { }

        if (!localeFound)
        {
            try
            {
                locale = update.Message.From.LanguageCode;
                localeFound = true;
            }
            catch { }
        }

        if (!localeFound)
        {
            try
            {
                locale = update.EditedMessage.From.LanguageCode;
                localeFound = true;
            }
            catch { }
        }

        // Console.WriteLine("New update with " + locale);
        return locale;
    }
}
