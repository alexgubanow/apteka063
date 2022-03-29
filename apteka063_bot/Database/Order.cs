using System.ComponentModel.DataAnnotations;

namespace apteka063.Database;

public class Order
{
    public Order(long userId, OrderStatus status = OrderStatus.Filling, string contactPhone = "", string deliveryAddress = "")
    {
        UserId = userId;
        ContactPhone = contactPhone;
        DeliveryAddress = deliveryAddress;
        Status = status;
    }
    [Key]
    public int Id { get; set; }
    public long UserId { get; set; }
    public string Items { get; set; } = "";
    public OrderStatus Status { get; set; }
    public string ContactPhone { get; set; } = "";
    public string DeliveryAddress { get; set; } = "";
}
