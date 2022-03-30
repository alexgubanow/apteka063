using apteka063.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = apteka063.Database.User;

namespace apteka063.Handlers;

public partial class UpdateHandlers
{
    private async Task<Message?> OnQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, User user, CancellationToken cts = default)
    {
        if (callbackQuery.Data == "backtoMain")
        {
            return await ShowMainMenu(botClient, callbackQuery.Message!, Resources.Translation.MainMenu, cts, callbackQuery.Message!.MessageId);
        }
        else if (callbackQuery.Data!.Contains("section_"))
        {
            return await _menu.ItemsToOrder.ShowCategoriesAsync(botClient, callbackQuery, cts);
        }
        else if (callbackQuery.Data!.Contains("category_") == true)
        {
            return await _menu.ItemsToOrder.ShowItemsAsync(botClient, callbackQuery, cts);
        }
        else if (callbackQuery.Data!.Contains("item_") == true)
        {
            return await _menu.ItemsToOrder.OnItemReceivedAsync(botClient, callbackQuery, cts);
        }
        else if (callbackQuery.Data == "order")
        {
            return await _orderButton.OnOrderReplyReceived(botClient, callbackQuery, user.LastMessageSentId, cts);
        }
        else if (callbackQuery.Data.Contains("cancelOrder_"))
        {
            return await _orderButton.OnCancelOrder(botClient, callbackQuery, cts);
        }
        return await ShowMainMenu(botClient, callbackQuery.Message!, Resources.Translation.MainMenu, cts, callbackQuery.Message!.MessageId);
    }
}
