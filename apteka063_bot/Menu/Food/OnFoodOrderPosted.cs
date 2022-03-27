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

public partial class FoodMenu
{
    public static async Task OnOrderPosted(dbc.Apteka063Context db, ITelegramBotClient botClient, CallbackQuery callbackQuery, dbc.Order order, string foodList)
    {
        // Your order #%d has been posted
        // Details: .....
        // If nobody contacted you in 4 hours please use the follwing contacts
        // <list of contacts>
        string resultTranslatedText = "";
        resultTranslatedText += String.Format(Resources.Translation.OrderPosted, order.Id.ToString()) + "\n";
        resultTranslatedText += foodList + "\n";
        resultTranslatedText += Resources.Translation.TakeCare;

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoToMenu, "backtoMain") }
        };
        await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId, text: resultTranslatedText, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
