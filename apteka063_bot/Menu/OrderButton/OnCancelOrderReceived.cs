using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.menu;

public partial class OrderButton
{
    public async Task<Message> OnCancelOrder(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts)
    {
        int orderId = int.Parse(callbackQuery.Data!.Split('_', 2).Last());
        var order = await _db.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            _logger.LogError($"ORDER #{orderId} NOT FOUND");
        }
        else
        {
            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Order #{orderId} deleted", true, cancellationToken: cts);
        }
        return await bot.UpdateHandlers.ShowMainMenu(botClient, callbackQuery.Message!, Resources.Translation.MainMenu, cts, callbackQuery.Message!.MessageId);
    }
}
