namespace LogiTrack;

public class InventoryItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; } = 0.0m;
    public string Location { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public string DisplayInfo()
    {
        return $"{Name} - {Description} (Qty: {Quantity}) located at {Location}, last updated on {LastUpdated.ToShortDateString()}";
    }
}
