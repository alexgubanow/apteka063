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
    public async Task<Order> GetOrCreateOrderAsync(long userId)
    {
        var order = await Orders.FirstOrDefaultAsync(x => x.UserId == userId && x.Status == OrderStatus.Filling);
        if (order == null)
        {
            order = new(userId);
            await Orders.AddAsync(order);
            await SaveChangesAsync();
        }
        return order;
    }
    public async Task<Order?> GetOrderByIdAsync(int orderId) => await Orders.FirstOrDefaultAsync(x => x.Id == orderId);
}

public enum OrderStatus
{
    Filling, NeedApprove, InProgress, Declined, Closed
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