using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.Menu.OrderButton;

public partial class OrderButton
{
    public async Task<Message> OnOrderReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, int lastMessageSentId)
    {
        var order = await _db.GetOrCreateOrderForUserIdAsync(callbackQuery.From.Id);
        if (order.Items == null || order.Items == "")
        {
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, Resources.Translation.Order_is_empty_please_pick_some, true);
            return null!;
        }
        else
        {
            return await InitiateOrderAsync(botClient, callbackQuery, lastMessageSentId, order);
        }
    }
}
