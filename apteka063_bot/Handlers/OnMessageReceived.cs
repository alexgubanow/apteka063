using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.bot;

public partial class UpdateHandlers
{
    private async Task<Message> OnMessageReceivedAsync(ITelegramBotClient botClient, Message message, CancellationToken cts)
    {
        _logger.LogTrace($"Receive message type: {message.Type}");
        if (message.Type != MessageType.Text)
            return null!;
        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId, cts);
        var user = await _db.GetOrCreateUserAsync(message.From);
        var order = await _db.Orders.Where(x => x.UserId == user.Id && (x.Status == dbc.OrderStatus.NeedPhone || x.Status == dbc.OrderStatus.NeedAdress)).ToListAsync();
        if (order.Count > 0)
        {
            if (order.Count > 1)
            {
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "MORE THAN ONE ORDER FOUND", cancellationToken : cts);
                _logger.LogError($"MORE THAN ONE ORDER FOUND, FOR USER ID: {user.Id}");
                return null!;
            }
            try
            {
                return await _orderButton.DispatchStateAsync(botClient, message, order.First());
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception: {exception.Message}");
            }
        }

        var header = Resources.Translation.MainMenu;
        if (message.Text == "updb")
        {
            if (await _gsheet.SyncPillsAsync() == 0)
            {
                header += "\n" + Resources.Translation.DBUpdateFinished;
            }
            else
            {
                header += "\n" + Resources.Translation.DBUpdateFailed;
            }
        }
        return await ShowMainMenu(botClient, message, header, cts);
    }
    public static async Task<Message> ShowMainMenu(ITelegramBotClient botClient, Message message, string headerText, CancellationToken cts, int? messageId = null)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[] {
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Pills, "pills"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Food, "food"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Transport, "transport"), }, });

        if (messageId != null)
        {
            return await botClient.EditMessageTextAsync(chatId: message.Chat.Id, messageId: (int)messageId, text: headerText, replyMarkup: inlineKeyboard, cancellationToken: cts);
        }
        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: headerText, replyMarkup: inlineKeyboard, cancellationToken: cts);
    }
}
