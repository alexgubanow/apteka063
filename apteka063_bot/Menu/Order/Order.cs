using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.menu;

public partial class Order
{
    public const string ContactDetailsStateActionPhone = "Phone";
    public const string ContactDetailsStateActionAddress = "Address";
    private readonly ILogger<Order> _logger;
    private readonly dbc.Apteka063Context _db;
    private readonly Services.Gsheet _gsheet;
    public Order(ILogger<Order> logger, dbc.Apteka063Context db, Services.Gsheet gsheet)
    {
        _logger = logger;
        _db = db;
        _gsheet = gsheet;
    }
    public async Task DispatchStateAsync(ITelegramBotClient botClient, Message message, dbc.User user)
    {
        string[] statePath = user.State.Split('.');
        var order = _db.Orders!.FirstOrDefault(x => x.Id == int.Parse(statePath[1]));
        if (order == null)
        {
            throw new Exception($"Expected order #{statePath[1]} not found");
        }
        var handler = statePath[2] switch
        {
            ContactDetailsStateActionPhone => HandleContactPhoneAsync(botClient, message, user, order),
            ContactDetailsStateActionAddress => HandleContactAddressAsync(botClient, message, user, order),
            _ => throw new NotImplementedException()
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
        }
    }
    public async Task InitiateOrderAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, dbc.Order order)
    {
        var user = await dbc.User.GetUserAsync(_db, callbackQuery.From);
        user.State = $"Order.{order.Id}.{ContactDetailsStateActionPhone}";
        _db.Users!.Update(user);
        await _db.SaveChangesAsync();

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.Cancel, "backtoMain") }
        };
        await botClient.SendTextMessageAsync(chatId: callbackQuery.Message!.Chat.Id, text: Resources.Translation.ProvidePhoneNumber, replyMarkup: new InlineKeyboardMarkup(buttons));
    }
    public async Task HandleContactPhoneAsync(ITelegramBotClient botClient, Message message, dbc.User user, dbc.Order order)
    {
        order.ContactPhone = message.Text ?? "";
        _db.Orders!.Update(order);
        user.State = $"Order.{order.Id}.{ContactDetailsStateActionAddress}";
        _db.Users!.Update(user);
        await _db.SaveChangesAsync();

        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.Cancel, "backtoMain") }
        };
        await botClient.SendTextMessageAsync(chatId: message!.Chat.Id, text: Resources.Translation.ProvideDeliveryAddress, replyMarkup: new InlineKeyboardMarkup(buttons));
    }

    public async Task HandleContactAddressAsync(ITelegramBotClient botClient, Message message, dbc.User user, dbc.Order order)
    {
        order.DeliveryAddress = message.Text ?? "";
        _db.Orders!.Update(order);

        user.State = ""; // Reset the state
        _db.Users!.Update(user);
        await _db.SaveChangesAsync();

        await PublishOrderAsync(botClient, message);
    }
}
