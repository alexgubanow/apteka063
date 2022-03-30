using apteka063.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu.OrderButton;

public partial class OrderButton
{
    public async Task<Message> PublishOrderAsync(ITelegramBotClient botClient, Message message, int lastMessageSentId, Order order)
    {
        var itemsIds = order.Items!.Split(',');
        IQueryable<string> itemsNames = null!;
        if (order.Items.Contains('p'))
        {
            var items = _db.Pills!.Where(p => itemsIds.Contains(p.Id));
            itemsNames = items.Select(x => x.Name);
            foreach (var pill in items)
            {
                pill.FreezedAmout++;
            }
            await _db.SaveChangesAsync();
            await _gsheet.UpdateFreezedValues();
        }
        else
        {
            itemsNames = _db.Foods!.Where(p => itemsIds.Contains(p.Id)).Select(x => x.Name);
        }

        await _gsheet.PostOrder(order, message.From!.FirstName + ' ' + message.From.LastName, message.From.Username!, string.Join(", ", itemsNames));

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
        return await botClient.EditMessageTextAsync(message!.Chat.Id, lastMessageSentId, resultTranslatedText, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
