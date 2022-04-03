//using apteka063.Menu.ItemsToOrder;

using apteka063.Database;
using apteka063.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Configuration;
using apteka063.Constants;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu;

public class Menu
{
    private readonly ILogger<Menu> _logger;
    public readonly MyOrders MyOrders;
    private readonly Apteka063Context _db;
    private readonly bool _reportActivityIsEnabled = false;
    private readonly bool _reportIncidentIsEnabled = false;
    public Menu(ILogger<Menu> logger, MyOrders myOrders, Apteka063Context db)
    {
        _logger = logger;
        MyOrders = myOrders;
        _db = db;
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config == null)
        {
            _logger.LogCritical("failed to open app config");
        }
        else
        {
            if (config.AppSettings.Settings["ReportActivityIsEnabled"] != null)
            {
                _reportActivityIsEnabled = true;
            }
            if (config.AppSettings.Settings["ReportIncidentIsEnabled"] != null)
            {
                _reportIncidentIsEnabled = true;
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("ReportActivityIsEnabled");
            ConfigurationManager.RefreshSection("ReportIncidentIsEnabled");
        }
    }
    public async Task<Message?> ShowMainMenuAsync(ITelegramBotClient botClient, string headerText, long chatId, int? messageId = null, CancellationToken cts = default)
    {
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.Orders, CallbackDataConstants.MyOrders) }
        };
        if (_reportActivityIsEnabled)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.ReportActivity, CallbackDataConstants.ReportActivity) });
        }
        if (_reportIncidentIsEnabled)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.ReportActivity, CallbackDataConstants.ReportActivity) });
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.EmergencyContacts, CallbackDataConstants.EmergencyContacts) });
        
        var headerFromDB = (await _db.UserSettings.FirstOrDefaultAsync(x => x.Name == "Шапка главное меню"))?.Value;
        
        return await botClient.UpdateOrSendMessageAsync(_logger, $"{headerFromDB ?? ""}\n{headerText}", chatId, messageId, new InlineKeyboardMarkup(buttons), cts);
    }

    public async Task<Message?> ShowOrderTypesConfirmOrderReset(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cts = default)
    {
        var tgUser = callbackQuery.From;
        var order = await _db.Orders.FirstOrDefaultAsync(x => x.UserId == tgUser.Id && x.Status == OrderStatus.Filling, cancellationToken: cts);

        if (order == null)
        {
            return await MyOrders.ShowOrderTypesAsync(botClient, callbackQuery, cts);
        }

        var currentOrderMenuCallback = $"{CallbackDataConstants.OrderTypePrefix}{order.OrderType}";
        var buttons = new List<List<InlineKeyboardButton>>
        {
            new() { InlineKeyboardButton.WithCallbackData(Resources.Translation.OrderTypesWithResetOrder, CallbackDataConstants.OrderTypesWithResetOrder) },
            new() { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, currentOrderMenuCallback) }
        };

        var headerText = Resources.Translation.OrderTypesConfirmOrderReset;
        return await botClient.UpdateOrSendMessageAsync(_logger, headerText, callbackQuery.Message!.Chat.Id,
            callbackQuery.Message.MessageId, new InlineKeyboardMarkup(buttons), cts);
    }
}
