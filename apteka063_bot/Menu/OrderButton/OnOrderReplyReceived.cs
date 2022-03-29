using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.menu;

public partial class OrderButton
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
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "You haven't picked anything", true);
        }
        else
        {
            await InitiateOrderAsync(botClient, callbackQuery, order);
        }
    }
}
