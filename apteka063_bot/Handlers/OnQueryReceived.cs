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

public partial class UpdateHandlers
{
    private static async Task OnQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var user = await dbc.User.GetUserAsync(_db, callbackQuery.From);
        if (callbackQuery.Data == "backtoMain")
        {
            await OnMessageReceived(botClient, callbackQuery.Message!);
        }
        else if (callbackQuery.Data == "backtoPills" || callbackQuery.Data == "pills")
        {
            await OnPillsReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data.Contains("pillsCategory_") == true)
        {
            await OnPillsCategoryReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data.Contains("pill_") == true)
        {
            await OnPillsItemReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data == "order")
        {
            await OnOrderReplyReceived(botClient, callbackQuery);
        }
        else
        {
            await OnMessageReceived(botClient, callbackQuery.Message!);
        }
    }
    private static async Task OnPillsReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, dbc.Order? order = null)
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(callbackQuery.From.LanguageCode);

        await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
        order ??= _db.Orders.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = callbackQuery.From.Id };
            await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();
        }
        var orderPills = order.Pills?.Split(',');
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, "backtoMain") }
        };
        foreach (var pillCategory in Services.Gsheet.pillCategoriesMap)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(
                pillCategory.Key,
                $"pillsCategory_{pillCategory.Value}") });
        }
        await botClient.SendTextMessageAsync(chatId: callbackQuery.Message.Chat.Id, text: Resources.Translation.PickCategory, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
    private static async Task OnPillsCategoryReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, dbc.Order? order = null)
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(callbackQuery.From.LanguageCode);

        await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
        order ??= _db.Orders.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = callbackQuery.From.Id };
            await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();
        }
        var orderPills = order.Pills?.Split(',');
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, "backtoPills") }
        };
        var pillsDB = _db.Pills.Where(x => x.PillCategory == (dbc.PillCategories)Enum.Parse(typeof(dbc.PillCategories), callbackQuery.Data.ToString().Substring(14))).ToList();
        foreach (var pillDB in pillsDB)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(
                pillDB.Name + (orderPills != null && orderPills.Contains(pillDB.Id.ToString()) ? GEmojiSharp.Emoji.Emojify(" :ballot_box_with_check:") : ""),
                $"pill_{pillDB.Id}") });
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData("Order", "order") });
        await botClient.SendTextMessageAsync(chatId: callbackQuery.Message.Chat.Id, text: Resources.Translation.AvailableNow, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
    private static async Task OnPillsItemReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = _db.Orders.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = callbackQuery.From.Id };
            await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();
        }
        var pillID = callbackQuery.Data.ToString().Substring(5);
        var orderPillsList = order.Pills?.Split(',').ToList();
        if (orderPillsList != null)
        {
            if (orderPillsList.Contains(pillID))
            {
                orderPillsList.Remove(pillID);
            }
            else
            {
                orderPillsList.Add(pillID);
            }
        }
        else
        {
            orderPillsList = new() { pillID };
        }
        order.Pills = string.Join(',', orderPillsList);
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
        await OnPillsReplyReceived(botClient, callbackQuery, order);
    }
    private static async Task OnOrderReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = _db.Orders.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = callbackQuery.From.Id };
            await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();
        }
        if (order.Pills == null)
        {
            await OnPillsReplyReceived(botClient, callbackQuery, order);
        }
        var pillsList = "";
        try
        {
            foreach (var pill in order.Pills.Split(','))
            {
                pillsList += _db.Pills.Where(x => x.Id == int.Parse(pill)).FirstOrDefault()?.Name + ", ";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        await Services.Gsheet.PostOrder(order.Id.ToString(), callbackQuery.From.FirstName + ' ' + callbackQuery.From.LastName, pillsList);
        await OnPillsReplyReceived(botClient, callbackQuery, order);
    }
}
