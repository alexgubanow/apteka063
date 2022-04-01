using apteka063.Database;
using apteka063.Extensions;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = apteka063.Database.User;

namespace apteka063.Handlers;

public partial class UpdateHandlers
{
    private async Task<Message?> OnQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, User user, CancellationToken cts = default)
    {
        if (callbackQuery.Data == "main")
        {
            return await _menu.ShowMainMenuAsync(botClient, Resources.Translation.MainMenu, callbackQuery.Message!.Chat.Id, callbackQuery.Message!.MessageId, cts);
        }
        if (callbackQuery.Data == "myOrders")
        {
            return await _menu.MyOrders.ShowMyOrdersAsync(botClient, callbackQuery, cts);
        }
        if (callbackQuery.Data == "OrderTypes")
        {
            return await _menu.MyOrders.ShowOrderTypesAsync(botClient, callbackQuery, cts);
        }
        else if (callbackQuery.Data!.Contains("orderType_"))
        {
            return await _menu.MyOrders.ShowCategoriesAsync(botClient, callbackQuery, cts);
        }
        else if (callbackQuery.Data!.Contains("category_") == true)
        {
            return await _menu.MyOrders.ShowItemsAsync(botClient, callbackQuery, cts: cts);
        }
        else if (callbackQuery.Data!.Contains("item_") == true)
        {
            return await _menu.MyOrders.OnItemReceivedAsync(botClient, callbackQuery, cts);
        }
        else if (callbackQuery.Data == "order")
        {
            return await _orderButton.OnOrderReplyReceived(botClient, callbackQuery, user.LastMessageSentId, cts);
        }
        else if (callbackQuery.Data.Contains("cancelOrder_"))
        {
            return await _orderButton.OnCancelOrder(botClient, callbackQuery, cts);
        }
        else if (callbackQuery.Data == "emergencyContacts")
        {
            var emergencyContacts = (await _db.UserSettings.FirstOrDefaultAsync(x => x.Id == "emergencyContacts", cts))?.Value ?? "";
            var buttons = new List<List<InlineKeyboardButton>> { new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, "main") } };
            return await botClient.UpdateOrSendMessageAsync(_logger, emergencyContacts, callbackQuery.Message!.Chat.Id,
                callbackQuery.Message.MessageId, new InlineKeyboardMarkup(buttons), cts);
        }
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, Resources.Translation.NotImplemented, true, cancellationToken: cts);
        return null!;
    }
}
