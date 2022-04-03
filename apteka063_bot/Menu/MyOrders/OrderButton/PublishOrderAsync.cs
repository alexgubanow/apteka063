using apteka063.Database;
using apteka063.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace apteka063.Menu.OrderButton;

public partial class OrderButton
{
    public async Task<IQueryable<string>> PublishOrderAsync(Telegram.Bot.Types.User tgUser, Order order, CancellationToken cts = default)
    {
        var itemsIds = order.Items!.Split(',');
        IQueryable<string> itemsNames = null!;
        if (order.OrderType == OrderType.Pills)
        {
            var items = _db.ItemsToOrder!.Where(p => itemsIds.Contains(p.Id.ToString()));
            itemsNames = items.Select(x => x.Name);
            foreach (var pill in items)
            {
                pill.FreezedAmout++;
            }
            await _db.SaveChangesAsync(cts);
            await _gsheet.UpdateFreezedValues(cts);
        }
        else
        {
            itemsNames = _db.ItemsToOrder!.Where(p => itemsIds.Contains(p.Id.ToString())).Select(x => x.Name);
        }

        order.CreationDateTime = DateTime.Now;
        await _db.SaveChangesAsync(cts);
        await _gsheet.PostOrder(order, tgUser, string.Join(", ", itemsNames), cts);
        return itemsNames;
    }
}
