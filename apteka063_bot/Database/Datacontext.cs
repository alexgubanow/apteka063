using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace apteka063.Database;
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
    public DbSet<PillCategory> PillCategories { get; set; } = null!;
    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //=> optionsBuilder.ConfigureWarnings(c => 
    //c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug), 
    //    (RelationalEventId.CommandExecuted, LogLevel.Debug)));
    public Apteka063Context(DbContextOptions<Apteka063Context> options) : base(options) { }
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
    public async Task TruncatePillsAsync() => await Database.ExecuteSqlRawAsync("DELETE FROM Pills");
    public async Task TruncatePillCategoriesAsync() => await Database.ExecuteSqlRawAsync("DELETE FROM PillCategories");
    public async Task TruncateFoodAsync() => await Database.ExecuteSqlRawAsync("DELETE FROM Foods");
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
public class PillCategory
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
public class Pill
{
    Pill() { }
    public Pill(string id, IList<object> row)
    {
        Id = id;
        Name = row[0].ToString()!;
        PillCategoryName = row[1].ToString()!;
        FreezedAmout = int.Parse(row[2].ToString()!);
    }
    [Key]
    public string Id { get; set; }
    public string Name { get; set; } = "";
    public string PillCategoryName { get; set; } = "";
    public int FreezedAmout { get; set; }
}
public class Food
{
    Food() { }
    public Food(string id, IList<object> row)
    {
        Id = id;
        Name = row[0].ToString()!;
    }
    [Key]
    public string Id { get; set; }
    public string Name { get; set; } = "";
}
