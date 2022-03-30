using apteka063.Database;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu;

public class ItemsToOrder
{
    private readonly ILogger<ItemsToOrder> _logger;
    private readonly Apteka063Context _db;
    public ItemsToOrder(ILogger<ItemsToOrder> logger, Apteka063Context db)
    {
        _logger = logger;
        _db = db;
    }
    public async Task<Message> ShowCategoriesAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var section = callbackQuery.Data!.Split('_', 2).Last();
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, $"backtoMain") }
        };
        var categories = _db.ItemsCategories.Where(x => x.Section == section);
        foreach (var category in categories)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(category.Name, $"category_{category.Id}") });
        }
        return await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId,
            text: Resources.Translation.PickCategory, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
    public async Task<Message> ShowItemsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, string category = null, Order? order = null)
    {
        category ??= callbackQuery.Data!.Split('_', 2).Last();
        var section = await _db.ItemsCategories.FindAsync(category);
        order ??= await _db.GetOrCreateOrderForUserIdAsync(callbackQuery.From.Id);
        var orderItems = order.Items?.Split(',');
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, $"section_{section.Section}") }
        };
        var items = _db.ItemsToOrder!.Where(x => x.CategoryId == category).ToList();
        foreach (var item in items)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(
                item.Name + (orderItems != null && orderItems.Contains(item.Id) ? GEmojiSharp.Emoji.Emojify(" :ballot_box_with_check:") : ""),
                $"item_{item.Id}") });
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.Order, "order") });
        return await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId, 
            text: Resources.Translation.AvailableNow, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
    public async Task<Message> OnItemReceivedAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = await _db.GetOrCreateOrderForUserIdAsync(callbackQuery.From.Id);
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
        await _db.SaveChangesAsync();
        var item = await _db.ItemsToOrder.FindAsync(itemId);
        return await ShowItemsAsync(botClient, callbackQuery, item!.CategoryId, order);
    }
}
