using Microsoft.Extensions.Logging;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace apteka063.bot;

public partial class UpdateHandlers
{
    private readonly ILogger<UpdateHandlers> _logger;
    private readonly dbc.Apteka063Context _db;
    private readonly Services.Gsheet _gsheet;
    public UpdateHandlers(ILogger<UpdateHandlers> logger, dbc.Apteka063Context db, Services.Gsheet gsheet)
    {
        _logger = logger;
        _db = db;
        _gsheet = gsheet;
    }
    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        _logger.LogError(ErrorMessage);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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

    private Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        _logger.LogWarning($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }
    private static string GetLangaugeCodeFromUpdate(Update update)
    {
        string locale = "";
        if (update.CallbackQuery != null)
        {
            locale = update.CallbackQuery.From.LanguageCode ?? "";
        }
        else if (locale == "" && update.Message != null && update.Message.From != null)
        {
            locale = update.Message.From.LanguageCode ?? "";
        }
        else if (locale == "" && update.EditedMessage != null && update.EditedMessage.From != null)
        {
            locale = update.EditedMessage.From.LanguageCode ?? "";
        }
        else
        {
            locale = "ru";
        }
        return locale;
    }
}
