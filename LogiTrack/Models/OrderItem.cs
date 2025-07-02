using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogiTrack.Models;

public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Order")]
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [ForeignKey("InventoryItem")]
    public int InventoryItemId { get; set; }
    public InventoryItem InventoryItem { get; set; } = null!;
}