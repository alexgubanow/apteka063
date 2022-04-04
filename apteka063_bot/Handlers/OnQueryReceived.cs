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
    private async Task<Message> OnQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, User user, CancellationToken cts = default)
    {
        if (callbackQuery.Data == "main")
        {
            return await _menu.ShowMainMenuAsync(botClient, Resources.Translation.MainMenu, callbackQuery.Message!, cts: cts);
        }
        else if (callbackQuery.Data == "myOrders")
        {
            return await _menu.MyOrders.ShowMyOrdersAsync(botClient, callbackQuery.Message!, user, cts);
        }
        else if (callbackQuery.Data == "OrderTypes")
        {
            return await _menu.MyOrders.ShowOrderTypesAsync(botClient, callbackQuery.Message!, cts);
        }
        else if (callbackQuery.Data!.Contains("orderType_"))
        {
            var orderType = (OrderType)Enum.Parse(typeof(OrderType), callbackQuery.Data!.Split('_', 2).Last());
            var order = await _db.Orders.FirstOrDefaultAsync(x => x.OrderType == orderType && x.UserId == user.Id && x.Status == OrderStatus.NeedOrderConfirmation, cts);
            if (order != null)
            {
                order.Status = OrderStatus.Filling;
                order.LastUpdateDateTime = DateTime.Now;
                await _db.SaveChangesAsync(cts);
            }
            return await _menu.MyOrders.ShowCategoriesAsync(botClient, callbackQuery.Message!, orderType, cts);
        }
        else if (callbackQuery.Data!.Contains("category_") == true)
        {
            var categoryId = int.Parse(callbackQuery.Data!.Split('_', 2).Last());
            return await _menu.MyOrders.ShowItemsAsync(botClient, callbackQuery, categoryId, cts: cts);
        }
        else if (callbackQuery.Data!.Contains("item_") == true)
        {
            return await _menu.MyOrders.OnItemReceivedAsync(botClient, callbackQuery, cts);
        }
        else if (callbackQuery.Data == "order")
        {
            return await _orderButton.OnOrderReplyReceived(botClient, callbackQuery, cts);
        }
        else if (callbackQuery.Data!.Contains("order_") == true)
        {
            return await _menu.MyOrders.ShowOrderDetailsAsync(botClient, callbackQuery, cts);
        }
        else if (callbackQuery.Data!.Contains("orderDelete_") == true)
        {
            return await _orderButton.DeleteOrder(botClient, callbackQuery, cts);
        }
        else if (callbackQuery.Data == "emergencyContacts")
        {
            var emergencyContacts = (await _db.UserSettings.FirstOrDefaultAsync(x => x.Name == "emergencyContacts", cts))?.Value ?? "";
            var buttons = new List<List<InlineKeyboardButton>> { new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(Resources.Translation.GoBack, "main") } };
            return await botClient.UpdateOrSendMessageAsync(_logger, emergencyContacts, callbackQuery.Message!, new InlineKeyboardMarkup(buttons), cts: cts);
        }
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, Resources.Translation.NotImplemented, true, cancellationToken: cts);
        return null!;
    }
}
