using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.menu;

public partial class OrderButton
{
    private readonly ILogger<OrderButton> _logger;
    private readonly dbc.Apteka063Context _db;
    private readonly Services.Gsheet _gsheet;
    public OrderButton(ILogger<OrderButton> logger, dbc.Apteka063Context db, Services.Gsheet gsheet)
    {
        _logger = logger;
        _db = db;
        _gsheet = gsheet;
    }
    public async Task<Message> DispatchStateAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, dbc.Order order)
    {
        var handler = order.Status switch
        {
            dbc.OrderStatus.NeedPhone => SaveContactPhoneAsync(botClient, message, lastMessageSentId, order),
            dbc.OrderStatus.NeedAdress => SaveContactAddressAsync(botClient, message, lastMessageSentId, order),
            _ => throw new NotImplementedException()
        };
        try
        {
            return await handler;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
        }
        return null!;
    }
    public async Task<Message> InitiateOrderAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, int lastMessageSentId, dbc.Order order)
    {
        order.Status = dbc.OrderStatus.NeedPhone;
        await _db.SaveChangesAsync();
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.Cancel, $"cancelOrder_{order.Id}") }
        };
        return await botClient.EditMessageTextAsync(callbackQuery.Message!.Chat.Id, lastMessageSentId, Resources.Translation.ProvidePhoneNumber, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
    public async Task<Message> SaveContactPhoneAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, dbc.Order order)
    {
        order.ContactPhone = message.Text ?? "";
        order.Status = dbc.OrderStatus.NeedAdress;
        await _db.SaveChangesAsync();
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.Cancel, $"cancelOrder_{order.Id}") }
        };
        return await botClient.EditMessageTextAsync(message!.Chat.Id, lastMessageSentId, Resources.Translation.ProvideDeliveryAddress, replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    public async Task<Message> SaveContactAddressAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, dbc.Order order)
    {
        order.DeliveryAddress = message.Text ?? "";
        order.ContactPhone = message.Text ?? "";
        order.Status = dbc.OrderStatus.NeedApprove;
        await _db.SaveChangesAsync();
        return await PublishOrderAsync(botClient, message, lastMessageSentId);
    }
}
