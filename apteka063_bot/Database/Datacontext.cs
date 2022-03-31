using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace apteka063.Database;
//after update of any class below need to run:
//Add-Migration DESCRIPTION_OF_CHANGE
//Update-Database
public class Apteka063ContextFactory : IDesignTimeDbContextFactory<Apteka063Context>
{
    public Apteka063Context CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<Apteka063Context>();
        optionsBuilder.UseSqlite(Apteka063Context.connString);
        return new Apteka063Context(optionsBuilder.Options);
    }
}
public class Apteka063Context : DbContext
{
    public readonly static string connString = $"Data Source={Path.Join(Environment.CurrentDirectory, "database.db")}";
    public DbSet<Contact> Contacts { get; set; } = null!;
    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ItemToOrder> ItemsToOrder { get; set; } = null!;
    public DbSet<ItemToOrderCategory> ItemsCategories { get; set; } = null!;
    public DbSet<UserSetting> UserSettings { get; set; } = null!;
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.ConfigureWarnings(c =>
    c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug),
        (RelationalEventId.CommandExecuted, LogLevel.Debug))).UseSqlite(connString);
    public Apteka063Context(DbContextOptions<Apteka063Context> options) : base(options) { }
    public async Task<Order> GetOrCreateOrderForUserIdAsync(long userId, string? orderType = null, CancellationToken cts = default)
    {
        var order = await Orders.FirstOrDefaultAsync(x => x.UserId == userId && (x.Status == OrderStatus.Filling || x.Status == OrderStatus.NeedPhone || x.Status == OrderStatus.NeedAdress), cts);
        if (order == null)
        {
            order = new(userId, orderType!);
            await Orders.AddAsync(order, cts);
            await SaveChangesAsync(cts);
        }
        return order;
    }
    public async Task<User> GetOrCreateUserAsync(Telegram.Bot.Types.User tgUser, CancellationToken cts = default)
    {
        var user = await Users.FindAsync(new object?[] { tgUser?.Id }, cancellationToken: cts);
        if (user == null)
        {
            user = new(tgUser!);
            await Users.AddAsync(user, cts);
            await SaveChangesAsync(cts);
        }
        return user;
    }
}
public enum OrderStatus
{
    Filling, NeedPhone, NeedAdress, NeedApprove, InProgress, Declined, Closed
}
public class ItemToOrder
{
    public ItemToOrder() { }
    public ItemToOrder(string id, string name, string _CategoryId, int freezedAmount)
    {
        Id = id;
        Name = name;
        CategoryId = _CategoryId;
        FreezedAmout = freezedAmount;
    }
    [Key]
    public string Id { get; set; }
    public string Name { get; set; } = "";
    public string CategoryId { get; set; } = "";
    public int FreezedAmout { get; set; }
}
public class ItemToOrderCategory
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; } = "";
    public string OrderType { get; set; } = "";
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
public class UserSetting
{
    [Key]
    public string Id { get; set; }
    public string Value { get; set; } = "";
}