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
            await ShowMainMenu(botClient, callbackQuery.Message!.Chat.Id, Resources.Translation.MainMenu, callbackQuery.Message.MessageId);
        }
        else if (callbackQuery.Data == "backtoPills" || callbackQuery.Data == "pills")
        {
            await PillsMenu.OnReplyReceived(_db, botClient, callbackQuery);
        }
        else if (callbackQuery.Data!.Contains("pillsCategory_") == true)
        {
            await PillsMenu.OnCategoryReplyReceived(_db, botClient, callbackQuery, callbackQuery.Data.ToString().Substring(14));
        }
        else if (callbackQuery.Data!.Contains("pill_") == true)
        {
            await PillsMenu.OnItemReplyReceived(_db, botClient, callbackQuery);
        }
        else if (callbackQuery.Data == "orderPills")
        {
            await PillsMenu.OnOrderReplyReceived(_db, botClient, callbackQuery);
        }
        else
        {
            await ShowMainMenu(botClient, callbackQuery.Message!.Chat.Id, Resources.Translation.MainMenu, callbackQuery.Message.MessageId);
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData("Order", "order") });
        await botClient.EditMessageTextAsync(chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId, text: Resources.Translation.PickCategory, replyMarkup: new InlineKeyboardMarkup(buttons));
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
        var pillCategory = _db.Pills.Where(x => x.Id == int.Parse(pillID)).FirstOrDefault().PillCategory;
        await OnPillsCategoryReplyReceived(botClient, callbackQuery, pillCategory.ToString(), order);
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
