using System.Linq;
using apteka063.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu.Pills;

public partial class PillsMenu
{
    public async Task<Message> OnCategoryReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, string  pillCategoryName, Order? order = null)
    {
        order ??= await _db.GetOrCreateOrderForUserIdAsync(callbackQuery.From.Id);
        var orderPills = order.Items?.Split(',');
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, "backtoPills") }
        };
        var pillsDB = _db.Pills!.Where(x => x.PillCategoryName == pillCategoryName).ToList();
        foreach (var pillDB in pillsDB)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(
                pillDB.Name + (orderPills != null && orderPills.Contains(pillDB.Id) ? GEmojiSharp.Emoji.Emojify(" :ballot_box_with_check:") : ""),
                $"pill_{pillDB.Id}") });
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData("Order", "order") });
        return await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId, text: Resources.Translation.PickCategory, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
