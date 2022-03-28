using System.Globalization;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.bot;

public partial class PillsMenu
{
    public static async Task OnOrderPosted(dbc.Apteka063Context db, ITelegramBotClient botClient, Message message)
    {
        var order = db.Orders!.Where(x => x.UserId == message.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = message.From.Id };
            await db.Orders!.AddAsync(order);
            await db.SaveChangesAsync();
        }
        if (order.Items == null || order.Items == "")
        {
            //await OnReplyReceived(db, botClient, callbackQuery, order);
            // ToDo: Send message to user that he did NOT do any order and return back Pills menu
            return;
        }

        var pillIds = order.Items!.Split(',').Select(x => int.Parse(x));
        var pillsNames = db.Pills!.Where(p => pillIds.Contains(p.Id)).Select(x => x.Name);
        var pillsList = string.Join(", ", pillsNames);

        await Services.Gsheet.PostOrder(order, message.From.FirstName + ' ' + message.From.LastName, message.From.Username!, pillsList);


        // Your order #%d has been posted
        // Details: .....
        // If nobody contacted you in 4 hours please use the follwing contacts
        // <list of contacts>
        string resultTranslatedText = "";
        resultTranslatedText += String.Format(Resources.Translation.OrderPosted, order.Id.ToString()) + "\n";
        resultTranslatedText += pillsList + "\n";
        resultTranslatedText += Resources.Translation.TakeCare;

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoToMenu, "backtoMain") }
        };
        await botClient.SendTextMessageAsync(chatId: message!.Chat.Id, text: resultTranslatedText); // Send as message to save for user
        await botClient.SendTextMessageAsync(chatId: message!.Chat.Id, text: Resources.Translation.GoToMenu, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
