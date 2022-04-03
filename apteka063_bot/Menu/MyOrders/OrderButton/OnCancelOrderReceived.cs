using apteka063.Handlers;
using apteka063.Resources;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.Menu.OrderButton;

public partial class OrderButton
{
    public async Task<Message> DeleteOrder(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
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
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"{Translation.OrderNumber}{orderId} {Translation.deleted}", true, cancellationToken: cts);
        }
        return await _menu.ShowMainMenuAsync(botClient, Translation.MainMenu, callbackQuery.Message!, cts: cts);
    }
}
