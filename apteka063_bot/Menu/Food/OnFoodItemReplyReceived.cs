using apteka063.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.Menu.Food;

public partial class FoodMenu
{
    public static async Task OnItemReplyReceived(dbc.Apteka063Context db, ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = await db.Orders!.GetActiveOrderAsync(callbackQuery.From.Id);
        if (order == null)
        {
            order = new() { UserId = callbackQuery.From.Id };
            await db.Orders!.AddAsync(order);
            await db.SaveChangesAsync();
        }

        var foodId = callbackQuery.Data!.Split('_', 2).Last();
        var orderFoodList = order.Items?.Split(',').ToList();
        if (orderFoodList != null)
        {
            if (orderFoodList.Contains(foodId))
            {
                orderFoodList.Remove(foodId);
            }
            else
            {
                orderFoodList.Add(foodId);
            }
        }
        else
        {
            orderFoodList = new() { foodId };
        }
        order.Items = string.Join(',', orderFoodList);
        db.Orders!.Update(order);
        await db.SaveChangesAsync();
        await OnReplyReceived(db, botClient, callbackQuery, order);
    }
}
