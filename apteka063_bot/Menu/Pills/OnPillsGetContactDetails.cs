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
    public const string pillsDetailsStateName = "Pills";

    public const string pillsDetailsStateActionInitial = "Phone";
    public const string pillsDetailsStateActionAddress = "Address";

    public static async Task getContactDetails(ITelegramBotClient botClient, Message message, dbc.Apteka063Context db, dbc.User user, string state)
    {
        var handler = state switch
        {
            pillsDetailsStateActionInitial => PillsMenu.handleContactPhone(botClient, message, db, user),
            pillsDetailsStateActionAddress => PillsMenu.handleContactAddress(botClient, message, db, user)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
        }
    }

    public static async Task getContactStart(ITelegramBotClient botClient, CallbackQuery callbackQuery, dbc.Apteka063Context db)
    {
        var user = await dbc.User.GetUserAsync(db, callbackQuery.From);
        user.State = pillsDetailsStateName + "." + pillsDetailsStateActionInitial;
        db.Users!.Update(user);
        await db.SaveChangesAsync();

        // Please provide the Contact Phone Number
        string resultTranslatedText = Resources.Translation.ProvidePhoneNumber;

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.Cancel, "backtoMain") }
        };
        await botClient.SendTextMessageAsync(chatId: callbackQuery.Message!.Chat.Id, text: resultTranslatedText, replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    public static async Task handleContactPhone(ITelegramBotClient botClient, Message message, dbc.Apteka063Context db, dbc.User user)
    {
        // ToDo: save phone to the order
        string phoneNumber = message.Text.ToString();
        updateOrderForUserWithPhoneNumber(db, user, phoneNumber);

        user.State = pillsDetailsStateName + "." + pillsDetailsStateActionAddress;
        db.Users!.Update(user);
        await db.SaveChangesAsync();

        string resultTranslatedText = Resources.Translation.ProvideDeliveryAddress;

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.Cancel, "backtoMain") }
        };
        await botClient.SendTextMessageAsync(chatId: message!.Chat.Id, text: resultTranslatedText, replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    public static async Task handleContactAddress(ITelegramBotClient botClient, Message message, dbc.Apteka063Context db, dbc.User user)
    {
        // ToDo: save delivery address to the order
        string deliveryAddress = message.Text.ToString();
        updateOrderForUserWithDeliveryAddress(db, user, deliveryAddress);

        user.State =""; // Reset the state
        db.Users!.Update(user);
        await db.SaveChangesAsync();

        await OnOrderPosted(db, botClient, message);
    }

    private static void updateOrderForUserWithPhoneNumber(dbc.Apteka063Context db, dbc.User user, string phoneNumber)
    {
        var order = db.Orders!.Where(x => x.UserId == user.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = user.Id };
            db.Orders!.AddAsync(order);
            db.SaveChangesAsync();
        }

        order.ContactPhone = phoneNumber;
    }

    private static void updateOrderForUserWithDeliveryAddress(dbc.Apteka063Context db, dbc.User user, string deliveryAddress)
    {
        var order = db.Orders!.Where(x => x.UserId == user.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = user.Id };
            db.Orders!.AddAsync(order);
            db.SaveChangesAsync();
        }

        order.DeliveryAddress = deliveryAddress;
    }
}
