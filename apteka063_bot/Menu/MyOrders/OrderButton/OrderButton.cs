using apteka063.Database;
using apteka063.Extensions;
using apteka063.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = apteka063.Database.User;

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
            OrderStatus.NeedOrderConfirmation => ConfirmOrderAsync(botClient, message, lastMessageSentId, order, cts),
            OrderStatus.NeedUserPhone => SaveUserPhoneAsync(botClient, message, lastMessageSentId, order, cts),
            OrderStatus.NeedContactPhone => SaveContactPhoneAsync(botClient, message, lastMessageSentId, order, cts),
            OrderStatus.NeedContactName => SaveContactNameAsync(botClient, message, lastMessageSentId, order, cts),
            OrderStatus.NeedContactAddress => SaveContactAddressAsync(botClient, message, lastMessageSentId, order, cts),
            OrderStatus.NeedOrderComment => SaveOrderCommentAsync(botClient, message, lastMessageSentId, order, cts),
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
        string headerTxt = $"{Translation.OrderNumber}{order.Id}\n";
        var orderItemsList = JsonSerializer.Deserialize<List<ItemInCart>>(order.Items)!;
        foreach (var item in orderItemsList)
        {
            headerTxt += $"{item.Name} - {item.Amount}{Translation.pcs}\n";
        }
        headerTxt = headerTxt.Remove(headerTxt.Length - 1, 1);
        order.Status = OrderStatus.NeedOrderConfirmation;
        order.LastUpdateDateTime = DateTime.Now;
        await _db.SaveChangesAsync(cts);

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.EditOrder, $"orderType_{order.OrderType}") },
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.ProceedOrder, $"order") }
        };
        return await botClient.UpdateOrSendMessageAsync(_logger, headerTxt, message, new InlineKeyboardMarkup(buttons), cts: cts);
    }
    public async Task<Message> ConfirmOrderAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, Order order, CancellationToken cts = default)
    {
        var headerTxt = Translation.ProvidePhoneNumber;
        order.Status = OrderStatus.NeedContactPhone;
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == order.UserId, cts);
        if ((user!.Username ?? "") == "" && user!.PhoneNumber == "")
        {
            headerTxt = Translation.ProvideYourPhoneNumber;
            order.Status = OrderStatus.NeedUserPhone;
        }
        order.LastUpdateDateTime = DateTime.Now;
        await _db.SaveChangesAsync(cts);
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Cancel, $"orderType_{order.OrderType}") },
        };
        return await botClient.UpdateOrSendMessageAsync(_logger, headerTxt, message, new InlineKeyboardMarkup(buttons), cts: cts);
    }
    public async Task<Message> SaveUserPhoneAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, Order order, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Cancel, $"orderType_{order.OrderType}") },
        };
        if (message.Text == null || message.Text == "")
        {
            return await botClient.UpdateOrSendMessageAsync(_logger, $"{Translation.Something_went_wrong_Please_correct}\n{Translation.ProvideYourPhoneNumber}",
                message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);
        }
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == order.UserId, cts);
        user!.PhoneNumber = message.Text;
        order.LastUpdateDateTime = DateTime.Now;
        order.Status = OrderStatus.NeedContactName;
        await _db.SaveChangesAsync(cts);
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.ProvidePhoneNumber, message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);
    }
    public async Task<Message> SaveContactPhoneAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, Order order, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Cancel, $"orderType_{order.OrderType}") },
        };
        if (message.Text == null || message.Text == "")
        {
            return await botClient.UpdateOrSendMessageAsync(_logger, $"{Translation.Something_went_wrong_Please_correct}\n{Translation.ProvidePhoneNumber}",
                message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);
        }
        order.ContactPhone = message.Text;
        order.LastUpdateDateTime = DateTime.Now;
        order.Status = OrderStatus.NeedContactName;
        await _db.SaveChangesAsync(cts);
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.ProvideReceiverName, message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);
    }
    public async Task<Message> SaveContactNameAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, Order order, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Cancel, $"orderType_{order.OrderType}") },
        };
        if (message.Text == null || message.Text == "")
        {
            return await botClient.UpdateOrSendMessageAsync(_logger, $"{Translation.Something_went_wrong_Please_correct}\n{Translation.ProvideReceiverName}",
                message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);
        }
        order.ContactName = message.Text;
        order.LastUpdateDateTime = DateTime.Now;
        order.Status = OrderStatus.NeedContactAddress;
        await _db.SaveChangesAsync(cts);
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.ProvideDeliveryAddress, message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);
    }

    public async Task<Message> SaveContactAddressAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, Order order, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.NoComment, $"order") }
        };
        if (message.Text == null || message.Text == "")
        {
            return await botClient.UpdateOrSendMessageAsync(_logger, $"{Translation.Something_went_wrong_Please_correct}\n{Translation.ProvideDeliveryAddress}",
                message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);
        }
        order.DeliveryAddress = message.Text;
        order.LastUpdateDateTime = DateTime.Now;
        order.Status = OrderStatus.NeedOrderComment;
        await _db.SaveChangesAsync(cts);
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.ProvideOrderComment, message.Chat.Id, lastMessageSentId, new InlineKeyboardMarkup(buttons), cts: cts);

    }

    public async Task<Message> SaveOrderCommentAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, Order order, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Cancel, $"orderType_{order.OrderType}") },
        };
        order.OrderComment = message.Text != null ? message.Text : "-";
        order.OrderComment = order.OrderComment == Translation.ProvideOrderComment ? "-" : order.OrderComment;
        order.LastUpdateDateTime = DateTime.Now;
        order.Status = OrderStatus.InProgress;
        await _db.SaveChangesAsync(cts);

        var user = _db.Users.FirstOrDefault(x => x.Id == order!.UserId);

        var msgWeJustSent = await botClient.UpdateOrSendMessageAsync(_logger, Translation.Order_received_processing_please_wait, message.Chat.Id, lastMessageSentId, cts: cts);
        return await FinilizeOrder(botClient, user!, msgWeJustSent, order, cts);
    }

    public async Task<Message> FinilizeOrder(ITelegramBotClient botClient, User user, Message message, Order order, CancellationToken cts = default)
    {
        var orderDescription = await PublishOrderAsync(user, order, cts);

        // Your order #%d has been posted
        // Details: .....
        // If nobody contacted you in 4 hours please use the following contacts
        // <list of contacts>
        string resultTranslatedText =
            $"{Translation.OrderNumber}{order.Id} {Translation.HasBeenRegistered}\n" +
            $"{orderDescription}\n" +
            $"{Translation.TakeCare}";

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.GoToMenu, "main") }
        };
        return await botClient.UpdateOrSendMessageAsync(_logger, resultTranslatedText, message, new InlineKeyboardMarkup(buttons), cts: cts);
    }
}
