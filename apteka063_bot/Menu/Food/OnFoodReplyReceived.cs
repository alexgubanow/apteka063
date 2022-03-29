using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.menu;

public partial class FoodMenu
{
    public async Task OnReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, dbc.Order? order = null)
    {
        order ??= await _db.GetOrCreateOrderAsync(callbackQuery.From.Id);
        var orderFood = order.Items?.Split(',');
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, "backtoMain") }
        };
        var foodsDB = _db.Foods!.ToList();
        foreach (var foodInDb in foodsDB)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(
                foodInDb.Name + (orderFood != null && orderFood.Contains(foodInDb.Id.ToString()) ? GEmojiSharp.Emoji.Emojify(" :ballot_box_with_check:") : ""),
                $"food_{foodInDb.Id}") });
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData("Order", "order") });
        await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId, text: Resources.Translation.Food, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
