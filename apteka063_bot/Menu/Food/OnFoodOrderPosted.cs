using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.menu;

public partial class FoodMenu
{
    public async Task OnOrderPosted(ITelegramBotClient botClient, CallbackQuery callbackQuery, dbc.Order order, string foodList)
    {
        // Your order #%d has been posted
        // Details: .....
        // If nobody contacted you in 4 hours please use the follwing contacts
        // <list of contacts>
        string resultTranslatedText =
            $"{Resources.Translation.OrderNumber}{order.Id} {Resources.Translation.HasBeenRegistered}\n" +
            $"{foodList}\n" +
            $"{Resources.Translation.TakeCare}";

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoToMenu, "backtoMain") }
        };
        await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId, text: resultTranslatedText, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
