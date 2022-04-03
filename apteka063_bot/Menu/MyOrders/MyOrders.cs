using apteka063.Database;
using apteka063.Extensions;
using apteka063.Resources;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu;

public partial class MyOrders
{
    private readonly ILogger<MyOrders> _logger;
    private readonly Apteka063Context _db;
    public MyOrders(ILogger<MyOrders> logger, Apteka063Context db)
    {
        _logger = logger;
        _db = db;
    }
    public async Task<Message> ShowMyOrdersAsync(ITelegramBotClient botClient, Message message, Database.User user, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.GoBack, $"main") }
        };
        var headerText = Translation.You_dont_have_orders;
        var myOrders = _db.Orders.Where(x => x.UserId == user!.Id);
        if (myOrders.Any())
        {
            foreach (var order in myOrders)
            {
                buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(
                $"{Translation.OrderNumber}{order.Id} - {TranslationConverter.ToLocaleString(order.OrderType)} - {TranslationConverter.ToLocaleString(order.Status)}", $"order_{order.Id}") });
            }
            headerText = Translation.ActiveOrders;
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Translation.NewOrder, "OrderTypes") });
        return await botClient.UpdateOrSendMessageAsync(_logger, headerText, message, new InlineKeyboardMarkup(buttons), cts: cts);
    }
}
