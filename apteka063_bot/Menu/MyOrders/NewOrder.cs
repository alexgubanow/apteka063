using apteka063.Database;
using apteka063.Extensions;
using apteka063.Resources;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu;

public partial class MyOrders
{
    public async Task<Message?> ShowOrderTypesAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
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
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.Choose_order_type, callbackQuery.Message!.Chat.Id,
            callbackQuery.Message.MessageId, new InlineKeyboardMarkup(buttons), cts);
    }
    public async Task<Message?> ShowCategoriesAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
    {
        var orderType = (OrderType)Enum.Parse(typeof(OrderType), callbackQuery.Data!.Split('_', 2).Last());
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
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.PickCategory, callbackQuery.Message!.Chat.Id,
            callbackQuery.Message.MessageId, new InlineKeyboardMarkup(buttons), cts);
    }
    public async Task<Message?> ShowItemsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, int category, Order? order = null, CancellationToken cts = default)
    {
        var orderType = (await _db.ItemsCategories.FindAsync(new object?[] { category }, cancellationToken: cts))!.OrderType;
        order ??= await _db.GetOrCreateOrderForUserIdAsync(callbackQuery.From.Id, orderType, cts);
        var orderItems = order.Items?.Split(',');
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.GoBack, $"orderType_{orderType}") }
        };
        var items = _db.ItemsToOrder.Where(x => x.CategoryId == category);
        foreach (var item in items)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(
                item.Name + (orderItems != null && orderItems.Contains(item.Id.ToString()) ? GEmojiSharp.Emoji.Emojify(" :ballot_box_with_check:") : ""),
                $"item_{item.Id}") });
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.Order, "order") });
        return await botClient.UpdateOrSendMessageAsync(_logger, Translation.AvailableNow, callbackQuery.Message!.Chat.Id,
            callbackQuery.Message.MessageId, new InlineKeyboardMarkup(buttons), cts);
    }
    public async Task<Message?> OnItemReceivedAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
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
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(cts);
        return await ShowItemsAsync(botClient, callbackQuery, categoryId, order, cts);
    }
}
