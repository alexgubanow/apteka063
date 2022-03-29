using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace apteka063.dbc;
//after update of any class below need to run:
//Add-Migration DESCRIPTION_OF_CHANGE
//Update-Database
public class Apteka063Context : DbContext
{
    public DbSet<Contact> Contacts { get; set; } = null!;
    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Pill> Pills { get; set; } = null!;
    public DbSet<Food> Foods { get; set; } = null!;
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={Path.Join(Environment.CurrentDirectory, "database.db")}");
    public async Task<Order> GetOrCreateOrderForUserIdAsync(long userId)
    {
        var order = await Orders.FirstOrDefaultAsync(x => x.UserId == userId && (x.Status == OrderStatus.Filling || x.Status == OrderStatus.NeedPhone || x.Status == OrderStatus.NeedAdress));
        if (order == null)
        {
            order = new(userId);
            await Orders.AddAsync(order);
            await SaveChangesAsync();
        }
        return order;
    }
    public async Task<User> GetOrCreateUserAsync(Telegram.Bot.Types.User tgUser)
    {
        var user = await Users.FindAsync(tgUser?.Id);
        if (user == null)
        {
            user = new(tgUser!);
            await Users.AddAsync(user);
            await SaveChangesAsync();
        }
        return user;
    }
    public async Task<Order?> GetOrderByIdAsync(int orderId) => await Orders.FirstOrDefaultAsync(x => x.Id == orderId);
    public async Task UpdateOrderAsync(Order order)
    {
        Orders.Update(order);
        await SaveChangesAsync();
    }
}

public enum OrderStatus
{
    Filling, NeedPhone, NeedAdress, NeedApprove, InProgress, Declined, Closed
}
public class Contact : Telegram.Bot.Types.Contact
{
    [Key]
    public long Id { get; set; }
}
public class Location : Telegram.Bot.Types.Location
{
    [Key]
    public long Id { get; set; }
}
public enum PillCategories
{
    Heart, Stomach, Painkiller, Fever, Child, Women, Other
}
public class Pill
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public PillCategories PillCategory { get; set; }
}
public class Food
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}