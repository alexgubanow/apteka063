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
    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cts)
    {
        var errorMessage = exception is ApiRequestException apiRequestException
            ? $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}"
            : exception.ToString();
        _logger.LogError(errorMessage);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cts = default)
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(GetLanguageCodeFromUpdate(update));

        var tgUser = update.Message?.From ?? update.EditedMessage?.From ?? update.CallbackQuery?.From;
        if (tgUser == null)
        {
            return;
        }
        var user = await _db.GetOrCreateUserAsync(tgUser, cts);
        Task<Message?> handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message            => OnMessageReceivedAsync(botClient, update.Message!, user, cts),
            UpdateType.EditedMessage      => OnMessageReceivedAsync(botClient, update.EditedMessage!, user, cts),
            UpdateType.CallbackQuery      => OnQueryReceived(botClient, update.CallbackQuery!, user, cts),
            //UpdateType.InlineQuery        => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
            //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
            _                             => UnknownUpdateHandlerAsync(botClient, update, user, cts)
        };
        Message? message = null!;
        try
        {
            message = await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(botClient, exception, cts);
        }
        if (message != null)
        {
            user.LastMessageSentId = message.MessageId;
            await _db.SaveChangesAsync(cts);
        }
        var userMessageId = update.Message?.MessageId ?? update.EditedMessage?.MessageId ?? -1;
        if (userMessageId != -1)
        {
            var chatId = update.Message != null ? update.Message.Chat.Id : update.EditedMessage != null ? update.EditedMessage.Chat.Id : -1;
            await botClient.DeleteMessageAsync(chatId, userMessageId, cts);
        }
    }

    private async Task<Message?> UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update, Database.User user, CancellationToken cts = default)
    {
        _logger.LogWarning($"Unknown update type: {update.Type}");
        return await _menu.ShowMainMenuAsync(botClient, Resources.Translation.MainMenu, update.Id, user.LastMessageSentId, cts);
    }
    
    private static string GetLanguageCodeFromUpdate(Update update)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                return update.Message!.From!.LanguageCode!;
            case UpdateType.InlineQuery:
                return update.InlineQuery!.From!.LanguageCode!;
            case UpdateType.ChosenInlineResult:
                return update.ChosenInlineResult!.From!.LanguageCode!;
            case UpdateType.CallbackQuery:
                return update.CallbackQuery!.From!.LanguageCode!;
            case UpdateType.EditedMessage:
                return update.EditedMessage!.From!.LanguageCode!;
            case UpdateType.ChannelPost:
                return update.ChannelPost!.From!.LanguageCode!;
            case UpdateType.EditedChannelPost:
                return update.EditedChannelPost!.From!.LanguageCode!;
            case UpdateType.ShippingQuery:
                return update.ShippingQuery!.From!.LanguageCode!;
            case UpdateType.PreCheckoutQuery:
                return update.PreCheckoutQuery!.From!.LanguageCode!;
            case UpdateType.MyChatMember:
                return update.MyChatMember!.From!.LanguageCode!;
            case UpdateType.ChatMember:
                return update.ChatMember!.From!.LanguageCode!;
            case UpdateType.ChatJoinRequest:
                return update.ChatJoinRequest!.From!.LanguageCode!;
            case UpdateType.Poll:
            case UpdateType.PollAnswer:
            case UpdateType.Unknown:
            default:
                return "ru";
        }
    }
}
