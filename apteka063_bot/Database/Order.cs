using System.ComponentModel.DataAnnotations;

namespace apteka063.Database;

public class Order
{
    Order() { }
    public Order(long userId, OrderType orderType, OrderStatus status = OrderStatus.Filling, string contactPhone = "", string _ContactName = "", string deliveryAddress = "")
    {
        UserId = userId;
        OrderType = orderType;
        ContactPhone = contactPhone;
        ContactName = _ContactName;
        DeliveryAddress = deliveryAddress;
        Status = status;
        CreationDateTime = DateTime.MinValue;
        LastUpdateDateTime = DateTime.Now;
    }
    [Key]
    public int Id { get; set; }
    public long UserId { get; set; }
    public string Items { get; set; } = "";
    public OrderStatus Status { get; set; }
    public string ContactPhone { get; set; } = "";
    public string ContactName { get; set; } = "";
    public OrderType OrderType { get; set; }
    public string DeliveryAddress { get; set; } = "";
    /// <summary>
    /// When the order was finished. So this is Order DateTime.
    /// Will have `MinValue` until the order is submitted.
    /// </summary>
    public DateTime CreationDateTime { get; set; } = DateTime.MinValue;
    /// <summary>
    /// DateTime when order was modified last time.
    /// </summary>
    public DateTime LastUpdateDateTime { get; set; } = DateTime.MinValue;
}
