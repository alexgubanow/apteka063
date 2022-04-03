using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Extensions
{
    public static class ITelegramBotClientExtensions
    {
        public static async Task<Message> UpdateOrSendMessageAsync(this ITelegramBotClient botClient, ILogger _log, string text, Message message, InlineKeyboardMarkup? markup = null, CancellationToken cts = default)
        {
            return await botClient.UpdateOrSendMessageAsync(_log, text, message.Chat.Id, message.MessageId, markup, cts);
        }
        public static async Task<Message> UpdateOrSendMessageAsync(this ITelegramBotClient botClient, ILogger _log, string text, long chatId, int messageId, InlineKeyboardMarkup? markup = null, CancellationToken cts = default)
        {
            try
            {
                return await botClient.EditMessageTextAsync(chatId, messageId, text: text, replyMarkup: markup, cancellationToken: cts);
            }
            catch (Exception)
            {
                //_log.LogWarning(ex, $"Chat id:{chatId}, message id:{messageId}\noriginal error message:\n{ex.Message}");
            }
            try
            {
                await botClient.DeleteMessageAsync(chatId, messageId, cts);
            }
            catch (Exception)
            {
                //_log.LogWarning(ex, $"Chat id:{chatId}, message id:{messageId}\noriginal error message:\n{ex.Message}");
            }
            return await botClient.SendTextMessageAsync(chatId: chatId, text: text, replyMarkup: markup, cancellationToken: cts);
        }
    }
}
