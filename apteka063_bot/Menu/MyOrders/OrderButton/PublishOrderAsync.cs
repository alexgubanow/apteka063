using apteka063.Database;
using apteka063.Resources;
using System.Text.Json;

namespace apteka063.Menu.OrderButton;

public partial class OrderButton
{
    public async Task<string> PublishOrderAsync(User user, Order order, CancellationToken cts = default)
    {
        var orderItemsList = JsonSerializer.Deserialize<List<ItemInCart>>(order.Items)!;
        string orderDescription = "";
        if (order.OrderType == OrderType.Pills)
        {
            var items = _db.ItemsToOrder!.Where(p => orderItemsList.Select(x => x.Id).Contains(p.Id));
            foreach (var pill in items)
            {
                pill.FreezedAmout++;
            }
            await _db.SaveChangesAsync(cts);
            await _gsheet.UpdateFreezedValues(cts);
        }
        foreach (var item in orderItemsList)
        {
            orderDescription += $"{item.Name} - {item.Amount}{Translation.pcs}\n";
        }
        orderDescription = orderDescription.Remove(orderDescription.Length - 1, 1);
        order.CreationDateTime = DateTime.Now;
        await _db.SaveChangesAsync(cts);
        await _gsheet.PostOrder(order, user, orderDescription, cts);
        return orderDescription;
    }
}
