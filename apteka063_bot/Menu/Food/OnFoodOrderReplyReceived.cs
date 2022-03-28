using apteka063.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.Menu.Food;

public partial class FoodMenu
{
    public static async Task OnOrderReplyReceived(dbc.Apteka063Context db, ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = await db.Orders!.GetActiveOrderAsync(callbackQuery.From.Id);
        if (order == null)
        {
            order = new() { UserId = callbackQuery.From.Id };
            await db.Orders!.AddAsync(order);
            await db.SaveChangesAsync();
        }
        if (order.Items == null)
        {
            await OnReplyReceived(db, botClient, callbackQuery, order);
        }

        var foodIds = order.Items!.Split(',').Select(x => int.Parse(x));
        var foodsNames = db.Foods!.Where(f => foodIds.Contains(f.Id)).Select(x => x.Name);
        var foodList = string.Join(", " , foodsNames);

        await Services.Gsheet.PostOrder(order, callbackQuery.From.FirstName + ' ' + callbackQuery.From.LastName, callbackQuery.From.Username!, foodList);
        await OnOrderPosted(db, botClient, callbackQuery, order, foodList);
    }
}
