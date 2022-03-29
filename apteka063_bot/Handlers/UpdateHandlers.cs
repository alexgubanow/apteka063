using Microsoft.EntityFrameworkCore;
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
    private readonly menu.Menu _menu;
    private readonly menu.OrderButton _orderButton;
    public UpdateHandlers(ILogger<UpdateHandlers> logger, dbc.Apteka063Context db, Services.Gsheet gsheet, menu.Menu menu, menu.OrderButton order)
    {
        _logger = logger;
        _db = db;
        _gsheet = gsheet;
        _menu = menu;
        _orderButton = order;
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

        var tgUser = update.Message != null ? update.Message.From : update.EditedMessage != null ? update.EditedMessage.From : update.CallbackQuery != null ? update.CallbackQuery.From : null;
        if (tgUser == null)
        {
            return;
        }
        var user = await _db.GetOrCreateUserAsync(tgUser);
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message            => OnMessageReceivedAsync(botClient, update.Message!, user, cancellationToken),
            UpdateType.EditedMessage      => OnMessageReceivedAsync(botClient, update.EditedMessage!, user, cancellationToken),
            UpdateType.CallbackQuery      => OnQueryReceived(botClient, update.CallbackQuery!, user, cancellationToken),
            //UpdateType.InlineQuery        => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
            //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
            _                             => UnknownUpdateHandlerAsync(botClient, update)
        };
        Message message = null!;
        try
        {
            message = await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(botClient, exception, cancellationToken);
        }
        if (message != null)
        {
            user.LastMessageSentId = message.MessageId;
            await _db.SaveChangesAsync(cancellationToken);
        }
        var userMessageId = update.Message != null ? update.Message.MessageId : update.EditedMessage != null ? update.EditedMessage.MessageId : -1;
        if (userMessageId != -1)
        {
            var chatId = update.Message != null ? update.Message.Chat.Id : update.EditedMessage != null ? update.EditedMessage.Chat.Id : -1;
            await botClient.DeleteMessageAsync(chatId, userMessageId, cancellationToken);
        }
    }

    private Task<Message> UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        _logger.LogWarning($"Unknown update type: {update.Type}");
        return null!;
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
