//using apteka063.Menu.ItemsToOrder;

using apteka063.Database;
using apteka063.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu;

public class Menu
{
    private readonly ILogger<Menu> _logger;
    public readonly MyOrders MyOrders;
    private readonly Apteka063Context _db;
    private readonly bool ReportActivityIsEnabled = false;
    private readonly bool ReportIncidentIsEnabled = false;
    public Menu(ILogger<Menu> logger, MyOrders _MyOrders, Apteka063Context db)
    {
        _logger = logger;
        MyOrders = _MyOrders;
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
                ReportActivityIsEnabled = true;
            }
            if (config.AppSettings.Settings["ReportIncidentIsEnabled"] != null)
            {
                ReportIncidentIsEnabled = true;
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
            new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.Orders, "myOrders") }
        };
        if (ReportActivityIsEnabled)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.ReportActivity, "reportActivity") });
        }
        if (ReportIncidentIsEnabled)
        {
            buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.ReportActivity, "reportActivity") });
        }
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.EmergencyContacts, "emergencyContacts") });
        var headerFormDB = (await _db.UserSettings.FirstOrDefaultAsync(x => x.Name == "Шапка главное меню"))?.Value;
        return await botClient.UpdateOrSendMessageAsync(_logger, $"{headerFormDB ?? ""}\n{headerText}", chatId, messageId, new InlineKeyboardMarkup(buttons), cts);
    }
}
