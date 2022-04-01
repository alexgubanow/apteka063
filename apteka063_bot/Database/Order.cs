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
}
