using System.Globalization;
using apteka063.Database;
using apteka063.Menu.OrderButton;
using apteka063.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace apteka063.Handlers;

public partial class UpdateHandlers
{
    private readonly ILogger<UpdateHandlers> _logger;
    private readonly Apteka063Context _db;
    private readonly Gsheet _gsheet;
    private readonly Menu.Menu _menu;
    private readonly OrderButton _orderButton;
    public UpdateHandlers(ILogger<UpdateHandlers> logger, Apteka063Context db, Gsheet gsheet, Menu.Menu menu, OrderButton order)
    {
        _logger = logger;
        _db = db;
        _gsheet = gsheet;
        _menu = menu;
        _orderButton = order;
    }
    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception is ApiRequestException apiRequestException
            ? $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}"
            : exception.ToString();
        _logger.LogError(errorMessage);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(GetLanguageCodeFromUpdate(update));

        var tgUser = update.Message?.From ?? update.EditedMessage?.From ?? update.CallbackQuery?.From;
        if (tgUser == null)
        {
            return;
        }
        var user = await _db.GetOrCreateUserAsync(tgUser);
        Task<Message?> handler = update.Type switch
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
        Message? message = null!;
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
        
        
        var userMessageId = update.Message?.MessageId ?? update.EditedMessage?.MessageId ?? -1;
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
    
    private static string GetLanguageCodeFromUpdate(Update update)
    {
        if (update.CallbackQuery?.From?.LanguageCode != null)
        {
            return update.CallbackQuery.From.LanguageCode;
        }
        
        if (update.Message?.From?.LanguageCode != null)
        {
            return update.Message.From.LanguageCode;
        }

        if (update.EditedMessage?.From?.LanguageCode != null)
        {
            return update.EditedMessage.From.LanguageCode;
        }

        return "ru";
    }
}
