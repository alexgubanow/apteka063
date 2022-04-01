using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ItemToOrder> ItemsToOrder => Set<ItemToOrder>();
    public DbSet<ItemToOrderCategory> ItemsCategories => Set<ItemToOrderCategory>();
    public DbSet<UserSetting> UserSettings => Set<UserSetting>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.ConfigureWarnings(c =>
    c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug),
        (RelationalEventId.CommandExecuted, LogLevel.Debug))).UseSqlite(connString);
    public Apteka063Context(DbContextOptions<Apteka063Context> options) : base(options) { }
    public async Task<Order> GetOrCreateOrderForUserIdAsync(long userId, OrderType orderType, CancellationToken cts = default)
    {
        var order = await Orders.FirstOrDefaultAsync(x => x.UserId == userId && x.OrderType == orderType &&
            (x.Status == OrderStatus.Filling || x.Status == OrderStatus.NeedContactPhone || x.Status == OrderStatus.NeedContactName || x.Status == OrderStatus.NeedContactAddress), cts);
        if (order == null)
        {
            order = new(userId, orderType);
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
    Filling, NeedContactPhone, NeedContactName, NeedContactAddress, Canceled, InProgress, Declined, Closed, N_A
}
public enum OrderType
{
    Pills, Humaid, Transport, N_A
}
public class ItemToOrder
{
    public ItemToOrder() { }
    public ItemToOrder(string name, int _CategoryId, int freezedAmount)
    {
        Name = name;
        CategoryId = _CategoryId;
        FreezedAmout = freezedAmount;
    }
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int CategoryId { get; set; }
    public int FreezedAmout { get; set; }
}
public class ItemToOrderCategory
{
    public ItemToOrderCategory() { }
    public ItemToOrderCategory(string name, OrderType _OrderType)
    {
        Name = name;
        OrderType = _OrderType;
    }
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public OrderType OrderType { get; set; }
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
    public UserSetting() { }
    public UserSetting(string name, string _Value)
    {
        Name = name;
        Value = _Value;
    }
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
}