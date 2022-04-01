using apteka063.Database;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.Menu.OrderButton;

public partial class OrderButton
{
    public async Task<Message?> OnOrderReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, int lastMessageSentId, CancellationToken cts = default)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(x => x.UserId == callbackQuery.From.Id &&
            (x.Status == OrderStatus.Filling || x.Status == OrderStatus.NeedPhone || x.Status == OrderStatus.NeedAdress), cts);
        if (order == null)
        {
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, Resources.Translation.No_any_active_orders_found, true, cancellationToken: cts);
            return null!;
        }
        else if (order.Items == null || order.Items == "")
        {
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, Resources.Translation.Order_is_empty_please_pick_some, true, cancellationToken: cts);
            return null!;
        }
        else
        {
            return await InitiateOrderAsync(botClient, callbackQuery, lastMessageSentId, order, cts);
        }
    }
}
