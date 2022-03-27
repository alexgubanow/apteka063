using apteka063.dbc;
using Microsoft.EntityFrameworkCore;

namespace apteka063.Extensions;

public static class DbSetOrdersExtensions
{
    public static async Task<Order?> GetActiveOrderAsync(this DbSet<Order> orders, long userId)
    {
        return await orders.FirstOrDefaultAsync(x => x.UserId == userId && x.Status != OrderStatus.Closed);
    }
}