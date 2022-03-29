using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.menu;

public partial class FoodMenu
{
    public async Task OnItemReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = await _db.GetOrCreateOrderAsync(callbackQuery.From.Id);
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
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
        await OnReplyReceived(botClient, callbackQuery, order);
    }
}
