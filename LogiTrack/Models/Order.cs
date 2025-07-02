using System.ComponentModel.DataAnnotations;

namespace LogiTrack.Models;

public class Order
{
    [Key]
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime DatePlaced { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";

    // Navigation property for many-to-many relationship
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public void AddItem(InventoryItem item)
    {
        OrderItems.Add(new OrderItem { OrderId = this.Id, InventoryItemId = item.Id, InventoryItem = item });
    }

    public void RemoveItem(InventoryItem item)
    {
        var orderItem = OrderItems.FirstOrDefault(oi => oi.InventoryItemId == item.Id);
        if (orderItem != null)
            OrderItems.Remove(orderItem);
    }

    public decimal GetTotalPrice()
    {
        return OrderItems.Sum(oi => oi.InventoryItem?.Price ?? 0);
    }

    public string GetOrderSummary()
    {
        var itemDetails = string.Join(", ", OrderItems.Select(oi => oi.InventoryItem?.DisplayInfo()));
        return $"Order ID: {Id}, Customer: {CustomerName}, Date: {DatePlaced.ToShortDateString()}, Status: {Status}, Items: [{itemDetails}], Total Price: {GetTotalPrice():C}";
    }
}
