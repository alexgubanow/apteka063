using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.menu;

public partial class OrderButton
{
    public async Task OnOrderReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = await _db.GetOrCreateOrderAsync(callbackQuery.From.Id);
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
