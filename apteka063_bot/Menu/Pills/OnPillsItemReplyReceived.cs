using apteka063.Database;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.Menu.Pills;

public partial class PillsMenu
{
    public async Task<Message> OnItemReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = await _db.GetOrCreateOrderForUserIdAsync(callbackQuery.From.Id);
        var pillID = callbackQuery.Data![5..];
        var orderPillsList = order.Items?.Split(',').ToList();
        if (orderPillsList != null)
        {
            if (orderPillsList.Contains(pillID))
            {
                orderPillsList.Remove(pillID);
            }
            else
            {
                orderPillsList.Add(pillID);
            }
        }
        else
        {
            orderPillsList = new() { pillID };
        }
        order.Items = string.Join(',', orderPillsList);
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
        var pill = await _db.Pills.FirstOrDefaultAsync(x => x.Id == int.Parse(pillID));
        return await OnCategoryReplyReceived(botClient, callbackQuery, pill?.PillCategory ?? PillCategories.Other, order);
    }
}
