using apteka063.Database;
using apteka063.Extensions;
using apteka063.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu;

public partial class MyOrders
{
    private readonly ILogger<MyOrders> _logger;
    private readonly Apteka063Context _db;
    public MyOrders(ILogger<MyOrders> logger, Apteka063Context db)
    {
        _logger = logger;
        _db = db;
    }
    public async Task<Message> ShowMyOrdersAsync(ITelegramBotClient botClient, Message message, Database.User user, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.GoBack, $"main") }
        };
        var headerText = Translation.You_dont_have_orders;
        var myOrders = _db.Orders.Where(x => x.UserId == user!.Id);
        if (myOrders.Any())
        {
            foreach (var order in myOrders)
            {
                buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(
                $"{Translation.OrderNumber}{order.Id} - {TranslationConverter.ToLocaleString(order.OrderType)} - {TranslationConverter.ToLocaleString(order.Status)}", $"order_{order.Id}") });
            }
            headerText = Translation.ActiveOrders;
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.NewOrder, "OrderTypes") });
        return await botClient.UpdateOrSendMessageAsync(_logger, headerText, message, new InlineKeyboardMarkup(buttons), cts: cts);
    }
    public async Task<Message> ShowOrderDetailsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
    {
        var orderId = int.Parse(callbackQuery.Data!.Split('_', 2).Last());
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.GoBack, $"myOrders") }
        };
        var order = await _db.Orders.FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken: cts);
        string headerText = $"{Translation.OrderNumber}{orderId}\n";
        if (order!.Items != "")
        {
            var orderItemsList = JsonSerializer.Deserialize<List<ItemInCart>>(order!.Items)!;
            foreach (var item in orderItemsList)
            {
                headerText += $"{item.Name} - {item.Amount}{Translation.pcs}\n";
            }
            headerText = headerText.Remove(headerText.Length - 1, 1);
        }
        if (order.ContactPhone != "")
        {
            headerText += $"\n{order.ContactPhone}\n";
        }
        if (order.ContactName != "")
        {
            headerText += $"{order.ContactName}\n";
        }
        if (order.DeliveryAddress != "")
        {
            headerText += $"{order.DeliveryAddress}\n";
        }
        if (order.OrderComment != "")
        {
            headerText += $"{order.OrderComment}";
        }
        if (order.Status == OrderStatus.Filling || order.Status == OrderStatus.NeedOrderConfirmation || order.Status == OrderStatus.NeedUserPhone || 
            order.Status == OrderStatus.NeedContactPhone || order.Status == OrderStatus.NeedContactName || order.Status == OrderStatus.NeedContactAddress ||
            order.Status == OrderStatus.NeedOrderComment)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.RemoveThisOrder, $"orderDelete_{order.Id}") });
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.EditOrder, $"orderType_{order.OrderType}") });
        }
        return await botClient.UpdateOrSendMessageAsync(_logger, headerText, callbackQuery.Message!, new InlineKeyboardMarkup(buttons), cts: cts);
    }
}
