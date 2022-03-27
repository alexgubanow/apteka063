using System.Globalization;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.bot;

public partial class FoodMenu
{
    public static async Task OnItemReplyReceived(dbc.Apteka063Context db, ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = db.Orders!.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new() { UserId = callbackQuery.From.Id };
            await db.Orders!.AddAsync(order);
            await db.SaveChangesAsync();
        }
        var foodID = callbackQuery.Data!.ToString().Substring(5);
        var orderFoodList = order.Items?.Split(',').ToList();
        if (orderFoodList != null)
        {
            if (orderFoodList.Contains(foodID))
            {
                orderFoodList.Remove(foodID);
            }
            else
            {
                orderFoodList.Add(foodID);
            }
        }
        else
        {
            orderFoodList = new() { foodID };
        }
        order.Items = string.Join(',', orderFoodList);
        db.Orders!.Update(order);
        await db.SaveChangesAsync();
        await OnReplyReceived(db, botClient, callbackQuery, order);
    }
}
