namespace LogiTrack.Models;

public class Order
{
    private List<InventoryItem> items = new List<InventoryItem>();

    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime DatePlaced { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";

    public List<InventoryItem> Items { get => items; set => items = value; }

    public void AddItem(InventoryItem item)
    {
        items.Add(item);
    }

    public void RemoveItem(InventoryItem item)
    {
        items.Remove(item);
    }

    public decimal GetTotalPrice()
    {
        return items.Sum(item => item.Price);
    }

    public string GetOrderSummary()
    {
        var itemDetails = string.Join(", ", items.Select(item => item.DisplayInfo()));
        return $"Order ID: {Id}, Customer: {CustomerName}, Date: {DatePlaced.ToShortDateString()}, Status: {Status}, Items: [{itemDetails}], Total Price: {GetTotalPrice():C}";
    }
}
