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
    public static async Task OnOrderReplyReceived(dbc.Apteka063Context db, ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = db.Orders!.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = callbackQuery.From.Id };
            await db.Orders!.AddAsync(order);
            await db.SaveChangesAsync();
        }
        if (order.Items == null || order.Items == "")
        {
            await OnReplyReceived(db, botClient, callbackQuery, order);
        }
        
        var pillIds = order.Items!.Split(',').Select(x => int.Parse(x));
        var pillsNames = db.Pills!.Where(p => pillIds.Contains(p.Id)).Select(x => x.Name);
        var pillsList = string.Join(", " , pillsNames);
        
        await Services.Gsheet.PostOrder(order.Id.ToString(), callbackQuery.From.FirstName + ' ' + callbackQuery.From.LastName, callbackQuery.From.Username!, pillsList);
        await OnOrderPosted(db, botClient, callbackQuery, order, pillsList);
    }
}
