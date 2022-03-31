using apteka063.Database;
using apteka063.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = apteka063.Database.User;

namespace apteka063.Handlers;

public partial class UpdateHandlers
{
    private async Task<Message?> OnMessageReceivedAsync(ITelegramBotClient botClient, Message message, User user, CancellationToken cts = default)
    {
        _logger.LogTrace($"Receive message type: {message.Type}");
        if (message.Type != MessageType.Text)
            return null;
        
        var order = await _db.Orders.Where(x => x.UserId == user.Id && (x.Status == OrderStatus.NeedPhone || x.Status == OrderStatus.NeedAdress)).ToListAsync(cts);
        if (order.Count > 0)
        {
            if (order.Count > 1)
            {
                await botClient.UpdateOrSendMessageAsync(_logger, "MORE THAN ONE ORDER FOUND\nPlease contact admin", message!.Chat.Id, user.LastMessageSentId, cts: cts);
                _logger.LogError($"MORE THAN ONE ORDER FOUND, FOR USER ID: {user.Id}");
                return null!;
            }
            try
            {
                return await _orderButton.DispatchStateAsync(botClient, message, user.LastMessageSentId, order.First(), cts);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception: {exception.Message}");
            }
        }

        var header = Resources.Translation.MainMenu;
        if (message.Text == "updb")
        {
            if (await _gsheet.SyncAllTablesToDb(cts))
            {
                header += "\n" + Resources.Translation.DBUpdateFinished;
            }
            else
            {
                header += "\n" + Resources.Translation.DBUpdateFailed;
            }
        }
        return await _menu.ShowMainMenuAsync(botClient, message, header, user.LastMessageSentId, cts);
    }
}
