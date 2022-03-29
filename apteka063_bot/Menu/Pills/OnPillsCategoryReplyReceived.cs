using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.menu;

public partial class PillsMenu
{
    public async Task OnCategoryReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, string pillCategory, dbc.Order? order = null)
    {
        order ??= _db.Orders!.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new(callbackQuery.From.Id);
            await _db.Orders!.AddAsync(order);
            await _db.SaveChangesAsync();
        }
        var orderPills = order.Items?.Split(',');
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, "backtoPills") }
        };
        var pillsDB = _db.Pills!.Where(x => x.PillCategory == (dbc.PillCategories)Enum.Parse(typeof(dbc.PillCategories), pillCategory)).ToList();
        foreach (var pillDB in pillsDB)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(
                pillDB.Name + (orderPills != null && orderPills.Contains(pillDB.Id.ToString()) ? GEmojiSharp.Emoji.Emojify(" :ballot_box_with_check:") : ""),
                $"pill_{pillDB.Id}") });
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData("Order", "orderPills") });
        await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId, text: Resources.Translation.PickCategory, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
