using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.menu;

public partial class PillsMenu
{
    public async Task OnOrderReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = _db.Orders!.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new(callbackQuery.From.Id);
            await _db.Orders!.AddAsync(order);
            await _db.SaveChangesAsync();
        }
        if (order.Items == null || order.Items == "")
        {
            await OnReplyReceived(botClient, callbackQuery, order);
        }

        var pillIds = order.Items!.Split(',').Select(x => int.Parse(x));
        var pillsNames = _db.Pills!.Where(p => pillIds.Contains(p.Id)).Select(x => x.Name);
        var pillsList = string.Join(", ", pillsNames);

        //await _gsheet.PostOrder(order, callbackQuery.From.FirstName + ' ' + callbackQuery.From.LastName, callbackQuery.From.Username!, pillsList);
        await _order.InitiateOrderAsync(botClient, callbackQuery, order);
        //await OnOrderPosted(db, botClient, callbackQuery, order, pillsList);
    }
}
