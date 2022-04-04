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
    private async Task<Message> OnMessageReceivedAsync(ITelegramBotClient botClient, Message message, User user, CancellationToken cts = default)
    {
        _logger.LogTrace($"Receive message type: {message.Type}");
        if (message.Type != MessageType.Text)
            return null!;

        var order = await _db.Orders.FirstOrDefaultAsync(x => x.UserId == user.Id && (
            x.Status == OrderStatus.NeedUserPhone ||
            x.Status == OrderStatus.NeedContactPhone || x.Status == OrderStatus.NeedContactName ||
            x.Status == OrderStatus.NeedContactAddress || x.Status == OrderStatus.NeedOrderComment), cts);
        if (order != null)
        {
            //if (order.Count > 1)
            //{
            //    _logger.LogError($"MORE THAN ONE ORDER FOUND, FOR USER ID: {user.Id}");
            //    //return await botClient.UpdateOrSendMessageAsync(_logger, "MORE THAN ONE ORDER FOUND\nPlease contact admin", message, cts: cts);
            //}
            try
            {
                return await _orderButton.DispatchStateAsync(botClient, message, user.LastMessageSentId, order, cts);
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
        return await _menu.ShowMainMenuAsync(botClient, header, message, user.LastMessageSentId, cts);
    }
}
