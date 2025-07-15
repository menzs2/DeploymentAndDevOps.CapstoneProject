using LogiTrack.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack;

public class LogiTrackContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=logitrack.db");
        }
    }

    public LogiTrackContext(DbContextOptions<LogiTrackContext> options) : base(options) { }
    public LogiTrackContext() { }
}
