using apteka063.Database;
using apteka063.Extensions;
using apteka063.Resources;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu;

public partial class MyOrders
{
    public async Task<Message> ShowOrderTypesAsync(ITelegramBotClient botClient, Message message, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.GoBack, "myOrders") }
        };
        if (_db.ItemsCategories.Where(x => x.OrderType == OrderType.Pills).Any())
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Pills, $"orderType_{OrderType.Pills}") });
        }
        if (_db.ItemsCategories.Where(x => x.OrderType == OrderType.Humaid).Any())
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Humaid, $"orderType_{OrderType.Humaid}") });
        }
        if (_db.ItemsCategories.Where(x => x.OrderType == OrderType.Transport).Any())
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Transport, $"orderType_{OrderType.Transport}") });
        }
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.Choose_order_type, message, new InlineKeyboardMarkup(buttons), cts: cts);
    }
    public async Task<Message> ShowCategoriesAsync(ITelegramBotClient botClient, Message message, OrderType orderType, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.GoBack, "OrderTypes") }
        };
        var categories = _db.ItemsCategories.Where(x => x.OrderType == orderType);
        foreach (var category in categories)
        {
            if (_db.ItemsToOrder.Where(x => x.CategoryId == category.Id).Any())
            {
                buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(category.Name, $"category_{category.Id}") });
            }
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Order, "order") });
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.PickCategory, message, new InlineKeyboardMarkup(buttons), cts: cts);
    }
    public async Task<Message> ShowItemsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, int category, Order? order = null, CancellationToken cts = default)
    {
        var orderType = (await _db.ItemsCategories.FindAsync(new object?[] { category }, cancellationToken: cts))!.OrderType;
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.GoBack, $"orderType_{orderType}") }
        };
        var items = _db.ItemsToOrder.Where(x => x.CategoryId == category);
        order ??= await _db.Orders.FirstOrDefaultAsync(x => x.UserId == callbackQuery.From.Id && x.OrderType == orderType &&
            (x.Status == OrderStatus.Filling || x.Status == OrderStatus.NeedContactPhone || x.Status == OrderStatus.NeedContactName ||
             x.Status == OrderStatus.NeedContactAddress || x.Status == OrderStatus.NeedOrderComment || x.Status == OrderStatus.NeedOrderConfirmation), cts);
        var orderItems = order?.Items.Split(',');
        foreach (var item in items)
        {
            var checkMark = orderItems != null && orderItems.Contains(item.Id.ToString()) ? GEmojiSharp.Emoji.Emojify(" :ballot_box_with_check:") : "";
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(item.Name + checkMark, $"item_{item.Id}") });
        }
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.AvailableNow, callbackQuery.Message!, new InlineKeyboardMarkup(buttons), cts: cts);
    }
    public async Task<Message> OnItemReceivedAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
    {
        var itemId = callbackQuery.Data!.Split('_', 2).Last();
        var categoryId = (await _db.ItemsToOrder.FindAsync(new object?[] { int.Parse(itemId) }, cancellationToken: cts))!.CategoryId;
        var itemCategory = await _db.ItemsCategories.FindAsync(new object?[] { categoryId }, cancellationToken: cts);
        var order = await _db.GetOrCreateOrderForUserIdAsync(callbackQuery.From.Id, itemCategory!.OrderType, cts: cts);
        var orderItemsList = order.Items?.Split(',').ToList();
        if (orderItemsList != null)
        {
            if (orderItemsList.Contains(itemId))
            {
                orderItemsList.Remove(itemId);
            }
            else
            {
                orderItemsList.Add(itemId);
            }
        }
        else
        {
            orderItemsList = new() { itemId };
        }
        order.Items = string.Join(',', orderItemsList);
        order.LastUpdateDateTime = DateTime.Now;
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(cts);
        return await ShowItemsAsync(botClient, callbackQuery, categoryId, order, cts);
    }
}
