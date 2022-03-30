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
    public static async Task<Message> ShowSectionsAsync(ITelegramBotClient botClient, Message message, string headerText, CancellationToken cts = default, int? messageId = null)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[] {
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Pills, "section_pills"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Humaid, "section_humaid"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Transport, "section_transport"), }, });
        if (messageId != null)
        {
            try
            {
                return await botClient.EditMessageTextAsync(chatId: message.Chat.Id, messageId: (int)messageId, text: headerText, replyMarkup: inlineKeyboard, cancellationToken: cts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"message by id:\n{messageId} does not exist anymore\noriginal error message:\n{ex.Message}");
            }
        }
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: headerText, replyMarkup: inlineKeyboard, cancellationToken: cts);
    }
    public async Task<Message> ShowCategoriesAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
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
            text: Resources.Translation.PickCategory, replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: cts);
    }
    public async Task<Message> ShowItemsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default, string category = null, Order? order = null)
    {
        category ??= callbackQuery.Data!.Split('_', 2).Last();
        var section = await _db.ItemsCategories.FindAsync(new object?[] { category }, cancellationToken: cts);
        order ??= await _db.GetOrCreateOrderForUserIdAsync(callbackQuery.From.Id, cts);
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
            text: Resources.Translation.AvailableNow, replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: cts);
    }
    public async Task<Message> OnItemReceivedAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
    {
        var order = await _db.GetOrCreateOrderForUserIdAsync(callbackQuery.From.Id, cts);
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
        return await ShowItemsAsync(botClient, callbackQuery, cts, item!.CategoryId, order);
    }
}
