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
    public static async Task OnCategoryReplyReceived(dbc.Apteka063Context db, ITelegramBotClient botClient, CallbackQuery callbackQuery, string foodCategory, dbc.Order? order = null)
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
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, "backtoFood") }
        };
        var foodsDB = db.Foods!.Where(x => x.FoodCategory == (dbc.FoodCategories)Enum.Parse(typeof(dbc.FoodCategories), foodCategory)).ToList();
        foreach (var foodDB in foodsDB)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(
                foodDB.Name + (orderFood != null && orderFood.Contains(foodDB.Id.ToString()) ? GEmojiSharp.Emoji.Emojify(" :ballot_box_with_check:") : ""),
                $"food_{foodDB.Id}") });
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData("Order", "order") });
        await botClient.EditMessageTextAsync(chatId: callbackQuery.Message!.Chat.Id, messageId: callbackQuery.Message.MessageId, text: Resources.Translation.PickCategory, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
}
