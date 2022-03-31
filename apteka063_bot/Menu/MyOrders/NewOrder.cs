using apteka063.Database;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu;

public partial class MyOrders
{
    public async Task<Message> ShowOrderTypesAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[] {
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, "myOrders"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Pills, "orderType_pills"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Humaid, "orderType_humaid"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Transport, "orderType_transport"), }, });
        return await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId,
            text: "Choose order type", replyMarkup: inlineKeyboard, cancellationToken: cts);
    }
    public async Task<Message> ShowCategoriesAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
    {
        var orderType = callbackQuery.Data!.Split('_', 2).Last();
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, "myOrders") }
        };
        var categories = _db.ItemsCategories.Where(x => x.OrderType == orderType);
        foreach (var category in categories)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(category.Name, $"category_{category.Id}") });
        }
        return await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId,
            text: Resources.Translation.PickCategory, replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: cts);
    }
    public async Task<Message> ShowItemsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, string category = null!, Order? order = null, CancellationToken cts = default)
    {
        category ??= callbackQuery.Data!.Split('_', 2).Last();
        var orderType = (await _db.ItemsCategories.FindAsync(new object?[] { category }, cancellationToken: cts))?.OrderType;
        order ??= await _db.GetOrCreateOrderForUserIdAsync(callbackQuery.From.Id, orderType, cts);
        if (order.OrderType == null || order.OrderType == "")
        {
            order.OrderType = orderType!;
            await _db.SaveChangesAsync(cts);
        }
        var orderItems = order.Items?.Split(',');
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, $"orderType_{orderType}") }
        };
        var items = _db.ItemsToOrder.Where(x => x.CategoryId == category);
        foreach (var item in items)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(
                item.Name + (orderItems != null && orderItems.Contains(item.Id) ? GEmojiSharp.Emoji.Emojify(" :ballot_box_with_check:") : ""),
                $"item_{item.Id}") });
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.Order, "order") });
        return await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId, 
            text: Resources.Translation.AvailableNow, replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: cts);
    }
    public async Task<Message> OnItemReceivedAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
    {
        var order = await _db.GetOrCreateOrderForUserIdAsync(callbackQuery.From.Id, cts: cts);
        var itemId = callbackQuery.Data!.Split('_', 2).Last();
        var orderPillsList = order.Items?.Split(',').ToList();
        if (orderPillsList != null)
        {
            if (orderPillsList.Contains(itemId))
            {
                orderPillsList.Remove(itemId);
            }
            else
            {
                orderPillsList.Add(itemId);
            }
        }
        else
        {
            orderPillsList = new() { itemId };
        }
        order.Items = string.Join(',', orderPillsList);
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(cts);
        var item = await _db.ItemsToOrder.FindAsync(new object?[] { itemId }, cancellationToken: cts);
        return await ShowItemsAsync(botClient, callbackQuery, item!.CategoryId, order, cts);
    }
}
