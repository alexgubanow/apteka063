//using apteka063.Menu.ItemsToOrder;

using apteka063.Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu;

public class Menu
{
    private readonly ILogger<Menu> _logger;
    public readonly MyOrders MyOrders;
    public Menu(ILogger<Menu> logger, MyOrders _MyOrders)
    {
        _logger = logger;
        MyOrders = _MyOrders;
    }
    public async Task<Message> ShowMainMenuAsync(ITelegramBotClient botClient, Message message, string headerText, int? messageId = null, CancellationToken cts = default)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[] {
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Orders, "myOrders"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.ReportActivity, "reportActivity"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.ReportIncident, "reportIncident"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.EmergencyContacts, "emergencyContacts"), }, });
        return await botClient.UpdateOrSendMessageAsync(_logger, headerText, message.Chat.Id, messageId, inlineKeyboard, cts);
    }
}
