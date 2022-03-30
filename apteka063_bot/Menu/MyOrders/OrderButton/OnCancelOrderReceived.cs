using apteka063.Handlers;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.Menu.OrderButton;

public partial class OrderButton
{
    public async Task<Message> OnCancelOrder(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
    {
        int orderId = int.Parse(callbackQuery.Data!.Split('_', 2).Last());
        var order = await _db.Orders.FindAsync(new object?[] { orderId }, cancellationToken: cts);
        if (order == null)
        {
            _logger.LogError($"ORDER #{orderId} NOT FOUND");
        }
        else
        {
            _db.Orders.Remove(order);
            await _db.SaveChangesAsync(cts);
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Order #{orderId} deleted", true, cancellationToken: cts);
        }
        return await UpdateHandlers.ShowMainMenu(botClient, callbackQuery.Message!, Resources.Translation.MainMenu, callbackQuery.Message!.MessageId, cts);
    }
}
