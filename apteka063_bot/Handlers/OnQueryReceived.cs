using apteka063.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = apteka063.Database.User;

namespace apteka063.Handlers;

public partial class UpdateHandlers
{
    private async Task<Message> OnQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, User user, CancellationToken cts)
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
            return await _menu.Pills.OnCategoryReplyReceived(botClient, callbackQuery, (PillCategories)Enum.Parse(typeof(PillCategories), callbackQuery.Data.Split('_', 2).Last()));
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
