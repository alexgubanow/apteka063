using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.menu;

public partial class PillsMenu
{
    public async Task<Message> OnReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, "backtoMain") }
        };
        var PillCategoryIds = _db.Pills.Select(x => x.PillCategoryName);
        var pillCategories = _db.PillCategories.Where(x => PillCategoryIds.Contains(x.Name));
        foreach (var pillCategory in pillCategories)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(pillCategory.Name, $"pillsCategory_{pillCategory.Id}") });
        }
        return await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId, 
            text: Resources.Translation.PickCategory, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
