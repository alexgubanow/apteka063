using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.bot;

public partial class UpdateHandlers
{
    private async Task OnQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        if (callbackQuery.Data == "backtoMain")
        {
            await ShowMainMenu(botClient, callbackQuery.Message!, Resources.Translation.MainMenu, callbackQuery.Message!.MessageId);
        }
        else if (callbackQuery.Data == "backtoPills" || callbackQuery.Data == "pills")
        {
            await _menu.Pills.OnReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data!.Contains("pillsCategory_") == true)
        {
            await _menu.Pills.OnCategoryReplyReceived(botClient, callbackQuery, callbackQuery.Data.Split('_', 2).Last());
        }
        else if (callbackQuery.Data!.Contains("pill_") == true)
        {
            await _menu.Pills.OnItemReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data == "orderPills")
        {
            await _menu.Pills.OnOrderReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data == "backtoFood" || callbackQuery.Data == "food")
        {
            await _menu.Food.OnReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data!.Contains("food_") == true)
        {
            await _menu.Food.OnItemReplyReceived(botClient, callbackQuery);
        }
        else if (callbackQuery.Data == "orderFood")
        {
            await _menu.Food.OnOrderReplyReceived(botClient, callbackQuery);
        }
        else
        {
            await ShowMainMenu(botClient, callbackQuery.Message!, Resources.Translation.MainMenu, callbackQuery.Message!.MessageId);
        }
    }
}
