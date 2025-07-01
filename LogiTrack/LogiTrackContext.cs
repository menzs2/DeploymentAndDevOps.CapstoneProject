using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack;

public class LogiTrackContext: DbContext
{
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public LogiTrackContext(DbContextOptions<LogiTrackContext> options) : base(options)
    {
    }
}
