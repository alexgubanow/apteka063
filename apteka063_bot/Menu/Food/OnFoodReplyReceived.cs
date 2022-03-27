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
    public static async Task OnReplyReceived(dbc.Apteka063Context db, ITelegramBotClient botClient, CallbackQuery callbackQuery, dbc.Order? order = null)
    {

        order ??= db.Orders!.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = callbackQuery.From.Id };
            await db.Orders!.AddAsync(order);
            await db.SaveChangesAsync();
        }
        var orderFood = order.Pills?.Split(',');
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, "backtoMain") }
        };
        foreach (var foodCategory in Services.Gsheet.foodCategoriesMap)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(
                foodCategory.Key,
                $"foodCategory_{foodCategory.Value}") });
        }
        await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId, text: Resources.Translation.PickCategory, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
