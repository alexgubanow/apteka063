using apteka063.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.menu;

public partial class FoodMenu
{
    public async Task OnItemReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = await _db.Orders!.GetActiveOrderAsync(callbackQuery.From.Id);
        if (order == null)
        {
            order = new(callbackQuery.From.Id);
            await _db.Orders!.AddAsync(order);
            await _db.SaveChangesAsync();
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
        _db.Orders!.Update(order);
        await _db.SaveChangesAsync();
        await OnReplyReceived(botClient, callbackQuery, order);
    }
}
