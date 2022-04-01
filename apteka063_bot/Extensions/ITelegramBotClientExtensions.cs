using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Extensions
{
    public static class ITelegramBotClientExtensions
    {
        public static async Task<Message> UpdateOrSendMessageAsync(this ITelegramBotClient botClient, ILogger _log, string text, long chatId, int? messageId = null, InlineKeyboardMarkup? markup = null, CancellationToken cts = default)
        {
            if (messageId != null)
            {
                try
                {
                    return await botClient.EditMessageTextAsync(chatId: chatId, messageId: (int)messageId, text: text, replyMarkup: markup, cancellationToken: cts);
                }
                catch (Exception)
                {
                    //_log.LogWarning(ex, $"message by id:{messageId}\noriginal error message:\n{ex.Message}");
                }
            }
            return await botClient.SendTextMessageAsync(chatId: chatId, text: text, replyMarkup: markup, cancellationToken: cts);
        }
    }
}
