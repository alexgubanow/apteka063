using Telegram.Bot;
using Telegram.Bot.Types;

namespace apteka063.menu;

public partial class PillsMenu
{
    public async Task OnItemReplyReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var order = _db.Orders!.Where(x => x.UserId == callbackQuery.From.Id && x.Status != dbc.OrderStatus.Closed).FirstOrDefault();
        if (order == null)
        {
            order = new(callbackQuery.From.Id);
            await _db.Orders!.AddAsync(order);
            await _db.SaveChangesAsync();
        }
        var pillID = callbackQuery.Data!.ToString().Substring(5);
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
        _db.Orders!.Update(order);
        await _db.SaveChangesAsync();
        var pillCategory = _db.Pills!.Where(x => x.Id == int.Parse(pillID)).FirstOrDefault()!.PillCategory;
        await OnCategoryReplyReceived(botClient, callbackQuery, pillCategory.ToString(), order);
    }
}
