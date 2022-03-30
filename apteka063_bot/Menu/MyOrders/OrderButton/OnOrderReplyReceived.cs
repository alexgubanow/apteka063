using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.Menu.OrderButton;

public partial class OrderButton
{
    public async Task<Message> OnOrderReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, int lastMessageSentId, CancellationToken cts = default)
    {
        var order = await _db.GetOrCreateOrderForUserIdAsync(callbackQuery.From.Id, cts);
        if (order.Items == null || order.Items == "")
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
