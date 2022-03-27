using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace apteka063.dbc;
//after update of any class below need to run:
//Add-Migration DESCRIPTION_OF_CHANGE
//Update-Database
public class Apteka063Context : DbContext
{
    public DbSet<Contact>? Contacts { get; set; }
    public DbSet<Location>? Locations { get; set; }
    public DbSet<Order>? Orders { get; set; }
    public DbSet<User>? Users { get; set; }
    public DbSet<Pill>? Pills { get; set; }
    public DbSet<Food>? Foods { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={Path.Join(Environment.CurrentDirectory, "database.db")}");
}

public enum OrderStatus
{
    NeedApprove, InProgress, Declined, Closed
}

public class Order
{
    [Key]
    public int Id { get; set; }
    public long UserId { get; set; }
    public string? Pills { get; set; }
    public OrderStatus Status { get; set; }
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
public enum FoodCategories
{
    AdultFood, BabyFood
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
    public FoodCategories FoodCategory { get; set; }
}