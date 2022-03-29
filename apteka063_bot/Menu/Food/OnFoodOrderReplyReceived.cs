using apteka063.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.menu;

public partial class FoodMenu
{
    public async Task OnOrderReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = await _db.Orders!.GetActiveOrderAsync(callbackQuery.From.Id);
        if (order == null)
        {
            order = new(callbackQuery.From.Id);
            await _db.Orders!.AddAsync(order);
            await _db.SaveChangesAsync();
        }
        if (order.Items == null)
        {
            await OnReplyReceived(botClient, callbackQuery, order);
        }

        var foodIds = order.Items!.Split(',').Select(x => int.Parse(x));
        var foodsNames = _db.Foods!.Where(f => foodIds.Contains(f.Id)).Select(x => x.Name);
        var foodList = string.Join(", " , foodsNames);
        await _order.InitiateOrderAsync(botClient, callbackQuery, order);
    }
}
