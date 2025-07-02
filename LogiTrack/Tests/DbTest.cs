using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LogiTrack.Tests;

public class DbTest
{

    private DbContextOptions<LogiTrackContext> GetInMemoryOptions()
    {
        return new DbContextOptionsBuilder<LogiTrackContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void InventoryItems_AreAddedAndRetrievedCorrectly()
    {
        var options = GetInMemoryOptions();

        using (var context = new LogiTrackContext(options))
        {
            context.Database.EnsureCreated();
            context.InventoryItems.Add(new InventoryItem
            {
                Name = "Pallet Jack",
                Quantity = 12,
                Location = "Warehouse A"
            });
            context.InventoryItems.Add(new InventoryItem
            {
                Name = "Forklift",
                Quantity = 5,
                Location = "Warehouse B"
            });
            context.InventoryItems.Add(new InventoryItem
            {
                Name = "Hand Truck",
                Quantity = 20,
                Location = "Warehouse C"
            });
            context.SaveChanges();
        }

        using (var context = new LogiTrackContext(options))
        {
            Assert.Equal(3, context.InventoryItems.Count());
            Assert.True(context.InventoryItems.Any(i => i.Name == "Pallet Jack" && i.Quantity == 12 && i.Location == "Warehouse A"));
            Assert.True(context.InventoryItems.Any(i => i.Name == "Forklift" && i.Quantity == 5 && i.Location == "Warehouse B"));
            Assert.True(context.InventoryItems.Any(i => i.Name == "Hand Truck" && i.Quantity == 20 && i.Location == "Warehouse C"));
        }
    }

    [Fact]
    public void Order_IsAddedAndSummaryIsCorrect()
    {
        var options = GetInMemoryOptions();

        using (var context = new LogiTrackContext(options))
        {
            var item = new InventoryItem
            {
                Name = "Pallet Jack",
                Quantity = 12,
                Location = "Warehouse A"
            };
            context.InventoryItems.Add(item);
            context.SaveChanges();

            var order = context.Orders.Add(new Order
            {
                CustomerName = "John Doe",
                DatePlaced = DateTime.UtcNow,
                Status = "Pending",
                OrderItems = new System.Collections.Generic.List<OrderItem>()
            });
            order.Entity.AddItem(item);
            context.SaveChanges();
        }

        using (var context = new LogiTrackContext(options))
        {
            var order = context.Orders.Include(o => o.OrderItems).FirstOrDefault();
            Assert.NotNull(order);
            Assert.Equal("John Doe", order.CustomerName);
            Assert.Equal("Pending", order.Status);
            Assert.Single(order.OrderItems);
            Assert.Contains("John Doe", order.GetOrderSummary());
        }
    }
}
