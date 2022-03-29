using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.bot;

public partial class UpdateHandlers
{
    private async Task<Message> OnQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, dbc.User user, CancellationToken cts)
    {
        if (callbackQuery.Data == "backtoMain")
        {
            return await ShowMainMenu(botClient, callbackQuery.Message!, Resources.Translation.MainMenu, cts, callbackQuery.Message!.MessageId);
        }
        else if (callbackQuery.Data == "backtoPills" || callbackQuery.Data == "pills")
        {
            return await _menu.Pills.OnReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data!.Contains("pillsCategory_") == true)
        {
            var pillCategory = await _db.PillCategories.FindAsync(int.Parse(callbackQuery.Data.Split('_', 2).Last()));
            return await _menu.Pills.OnCategoryReplyReceived(botClient, callbackQuery, pillCategory!.Name);
        }
        else if (callbackQuery.Data!.Contains("pill_") == true)
        {
            return await _menu.Pills.OnItemReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data == "backtoFood" || callbackQuery.Data == "food")
        {
            return await _menu.Food.OnReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data!.Contains("food_") == true)
        {
            return await _menu.Food.OnItemReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data == "order")
        {
            return await _orderButton.OnOrderReplyReceived(botClient, callbackQuery, user.LastMessageSentId);
        }
        else if (callbackQuery.Data.Contains("cancelOrder_"))
        {
            return await _orderButton.OnCancelOrder(botClient, callbackQuery, cts);
        }
        return await ShowMainMenu(botClient, callbackQuery.Message!, Resources.Translation.MainMenu, cts, callbackQuery.Message!.MessageId);
    }
}
