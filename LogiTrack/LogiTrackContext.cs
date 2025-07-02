using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack;

public class LogiTrackContext: DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=logitrack.db");
    public LogiTrackContext(DbContextOptions<LogiTrackContext> options) : base(options)
    {
    }

    public LogiTrackContext()
    {
    }
}
