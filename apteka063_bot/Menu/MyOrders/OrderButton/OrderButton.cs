using apteka063.Database;
using apteka063.Extensions;
using apteka063.Resources;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu.OrderButton;

public partial class OrderButton
{
    private readonly ILogger<OrderButton> _logger;
    private readonly Apteka063Context _db;
    private readonly Services.Gsheet _gsheet;
    private readonly Menu _menu;
    public OrderButton(ILogger<OrderButton> logger, Apteka063Context db, Services.Gsheet gsheet, Menu menu)
    {
        _logger = logger;
        _db = db;
        _gsheet = gsheet;
        _menu = menu;
    }
    public async Task<Message> DispatchStateAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, Order order, CancellationToken cts = default)
    {
        if (message.Text?.StartsWith('/') == true)
        {
            message.Text = "";
        }
        var handler = order.Status switch
        {
            OrderStatus.NeedContactPhone => SaveContactPhoneAsync(botClient, message, lastMessageSentId, order, cts),
            OrderStatus.NeedContactName => SaveContactNameAsync(botClient, message, lastMessageSentId, order, cts),
            OrderStatus.NeedContactAddress => SaveContactAddressAsync(botClient, message, lastMessageSentId, order, cts),
            _ => throw new NotImplementedException()
        };
        try
        {
            return await handler;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);
        }
        return null!;
    }
    public async Task<Message> InitiateOrderAsync(ITelegramBotClient botClient, Message message, Order order, CancellationToken cts = default)
    {
        order.Status = OrderStatus.NeedContactPhone;
        await _db.SaveChangesAsync(cts);
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Cancel, $"cancelOrder_{order.Id}") }
        };
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.ProvidePhoneNumber, message, new InlineKeyboardMarkup(buttons), cts: cts);
    }
    public async Task<Message> SaveContactPhoneAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, Order order, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Cancel, $"cancelOrder_{order.Id}") }
        };
        if (message.Text == null || message.Text == "")
        {
            return await botClient.UpdateOrSendMessageAsync(_logger, $"{Translation.Something_went_wrong_Please_correct}\n{Translation.ProvidePhoneNumber}", 
                message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);
        }
        order.ContactPhone = message.Text;
        order.Status = OrderStatus.NeedContactName;
        await _db.SaveChangesAsync(cts);
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.ProvideReceiverName, message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);
    }
    public async Task<Message> SaveContactNameAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, Order order, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Cancel, $"cancelOrder_{order.Id}") }
        };
        if (message.Text == null || message.Text == "")
        {
            return await botClient.UpdateOrSendMessageAsync(_logger, $"{Translation.Something_went_wrong_Please_correct}\n{Translation.ProvideReceiverName}",
                message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);
        }
        order.ContactName = message.Text;
        order.Status = OrderStatus.NeedContactAddress;
        await _db.SaveChangesAsync(cts);
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.ProvideDeliveryAddress, message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);
    }
    public async Task<Message> SaveContactAddressAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, Order order, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Cancel, $"cancelOrder_{order.Id}") }
        };
        if (message.Text == null || message.Text == "")
        {
            return await botClient.UpdateOrSendMessageAsync(_logger, $"{Translation.Something_went_wrong_Please_correct}\n{Translation.ProvideDeliveryAddress}",
                message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);
        }
        order.DeliveryAddress = message.Text;
        order.LastUpdateDateTime = DateTime.Now;
        order.Status = OrderStatus.InProgress;
        await _db.SaveChangesAsync(cts);
        var msg = await botClient.UpdateOrSendMessageAsync(_logger, Translation.Order_received_processing_please_wait, message.Chat.Id, lastMessageSentId, cts: cts);
        return await PublishOrderAsync(botClient, msg, order, cts);
    }
}
