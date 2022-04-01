//using apteka063.Menu.ItemsToOrder;

using apteka063.Database;
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
    private readonly Apteka063Context _db;
    public Menu(ILogger<Menu> logger, MyOrders _MyOrders, Apteka063Context db)
    {
        _logger = logger;
        MyOrders = _MyOrders;
        _db = db;
    }
    public async Task<Message> ShowMainMenuAsync(ITelegramBotClient botClient, Message message, string headerText, int? messageId = null, CancellationToken cts = default)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[] {
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.Orders, "myOrders"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.ReportActivity, "reportActivity"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.ReportIncident, "reportIncident"), },
            new [] { InlineKeyboardButton.WithCallbackData(Resources.Translation.EmergencyContacts, "emergencyContacts"), }, });
        var headerFormDB = (await _db.UserSettings.FindAsync(new object?[] { "Шапка главное меню" }, cancellationToken: cts))?.Value;
        return await botClient.UpdateOrSendMessageAsync(_logger, $"{headerFormDB ?? ""}\n{headerText}", message.Chat.Id, messageId, inlineKeyboard, cts);
    }
}
