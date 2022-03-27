using apteka063.Menu.Food;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.bot;

public partial class UpdateHandlers
{
    private async Task OnQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var user = await dbc.User.GetUserAsync(_db, callbackQuery.From);
        if (callbackQuery.Data == "backtoMain")
        {
            await ShowMainMenu(botClient, callbackQuery.Message, Resources.Translation.MainMenu, callbackQuery.Message.MessageId);
        }
        else if (callbackQuery.Data == "backtoPills" || callbackQuery.Data == "pills")
        {
            await PillsMenu.OnReplyReceived(_db, botClient, callbackQuery);
        }
        else if (callbackQuery.Data!.Contains("pillsCategory_") == true)
        {
            await PillsMenu.OnCategoryReplyReceived(_db, botClient, callbackQuery, callbackQuery.Data.Split('_', 2).Last());
        }
        else if (callbackQuery.Data!.Contains("pill_") == true)
        {
            await PillsMenu.OnItemReplyReceived(_db, botClient, callbackQuery);
        }
        else if (callbackQuery.Data == "orderPills")
        {
            await PillsMenu.OnOrderReplyReceived(_db, botClient, callbackQuery);
        }
        else if (callbackQuery.Data == "backtoFood" || callbackQuery.Data == "food")
        {
            await FoodMenu.OnReplyReceived(_db, botClient, callbackQuery);
        }
        else if (callbackQuery.Data!.Contains("food_") == true)
        {
            await FoodMenu.OnItemReplyReceived(_db, botClient, callbackQuery);
        }
        else if (callbackQuery.Data == "orderFood")
        {
            await FoodMenu.OnOrderReplyReceived(_db, botClient, callbackQuery);
        }
        else
        {
            await ShowMainMenu(botClient, callbackQuery.Message, Resources.Translation.MainMenu, callbackQuery.Message.MessageId);
        }
    }
}
