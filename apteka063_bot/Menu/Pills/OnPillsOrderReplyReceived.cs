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
        if (order.Items == null)
        {
            await OnReplyReceived(db, botClient, callbackQuery, order);
        }
        var pillsList = "";
        try
        {
            foreach (var pill in order.Items!.Split(','))
            {
                pillsList += db.Pills!.Where(x => x.Id == int.Parse(pill)).FirstOrDefault()?.Name + ", ";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        await Services.Gsheet.PostOrder(order.Id.ToString(), callbackQuery.From.FirstName + ' ' + callbackQuery.From.LastName, callbackQuery.From.Username, pillsList);
        await OnOrderPosted(db, botClient, callbackQuery, order, pillsList);
    }
}
