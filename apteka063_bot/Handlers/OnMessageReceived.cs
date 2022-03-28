using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.bot;

public partial class UpdateHandlers
{
    private async Task OnMessageReceived(ITelegramBotClient botClient, Message message, dbc.Apteka063Context db)
    {
        _logger.LogTrace($"Receive message type: {message.Type}");
        if (message.Type != MessageType.Text)
            return;

        // Get User and check State
        var user = await dbc.User.GetUserAsync(_db, message.From);
        if (user.State != null && user.State != "")
        {
            // State will have to section: Request.Action
            string[] stateSplit = user.State.Split('.');
            var handler = stateSplit[0] switch
            {
                PillsMenu.pillsDetailsStateName => PillsMenu.getContactDetails(botClient, message, db, user, stateSplit[1])
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
            }

            return;
        }

        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
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
        await ShowMainMenu(botClient, message, header);
    }
    private async Task ShowMainMenu(ITelegramBotClient botClient, Message message, string headerText, int? messageId = null)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[] {
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Pills, "pills"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Food, "food"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Transport, "transport"), }, });

        if (messageId != null)
        {
            await botClient.EditMessageTextAsync(chatId: message.Chat.Id, messageId: messageId ?? 0, text: headerText, replyMarkup: inlineKeyboard);
        }
        else
        {
            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: headerText, replyMarkup: inlineKeyboard);
        }
    }
}
