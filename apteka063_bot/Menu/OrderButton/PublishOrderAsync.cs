using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.menu;

public partial class OrderButton
{
    public async Task PublishOrderAsync(ITelegramBotClient botClient, Message message)
    {
        var order = await _db.GetOrCreateOrderAsync(message.From.Id);
        if (order.Items == null || order.Items == "")
        {
            //await OnReplyReceived(db, botClient, callbackQuery, order);
            // ToDo: Send message to user that he did NOT do any order and return back Pills menu
            return;
        }

        var itemsIds = order.Items!.Split(',').Select(x => int.Parse(x));
        var itemsNames = _db.Pills!.Where(p => itemsIds.Contains(p.Id)).Select(x => x.Name);

        await _gsheet.PostOrder(order, message.From.FirstName + ' ' + message.From.LastName, message.From.Username!, string.Join(", ", itemsNames));


        // Your order #%d has been posted
        // Details: .....
        // If nobody contacted you in 4 hours please use the follwing contacts
        // <list of contacts>
        string resultTranslatedText =
            $"{Resources.Translation.OrderNumber}{order.Id} {Resources.Translation.HasBeenRegistered}\n" +
            $"{string.Join('\n', itemsNames)}\n" +
            $"{Resources.Translation.TakeCare}";

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoToMenu, "backtoMain") }
        };
        await botClient.SendTextMessageAsync(chatId: message!.Chat.Id, text: resultTranslatedText, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
