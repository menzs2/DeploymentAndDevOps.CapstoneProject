using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

namespace LogiTrack;

public class OrderService
{
    private readonly LogiTrackContext _context;
    private readonly IMemoryCache _inMemoryStoreCache;

    public OrderService(LogiTrackContext context, IMemoryCache inMemoryStoreCache)
    {
        _context = context;
        _inMemoryStoreCache = inMemoryStoreCache;
    }

    public List<Order> GetOrders()
    {
        if (_inMemoryStoreCache.TryGetValue("orders", out List<Order>? cachedOrders))
        {
            return cachedOrders ?? new List<Order>();
        }
        var orders = _context.Orders.AsNoTracking().ToList();
        _inMemoryStoreCache.Set("orders", orders, TimeSpan.FromMinutes(5));
        return orders;
    }

    public async Task<(bool result, string message)> InsertOrder(Order order)
    {
        var validation = ValidateOrder(order, null);
        if (validation.valid)
        {
            _context.Add(order);
            UpdateInventory(order.OrderItems);
            await _context.SaveChangesAsync();
        }
        return validation;
    }

    public async Task<(bool result, string[] message)> InsertOrder(List<Order> orders)
    {
        var messages = new List<string>();
        foreach (var order in orders)
        {
            var result = await InsertOrder(order);
            if (!result.result)
            {
                messages.Add(result.message);
            }
        }
        return (messages.Any(), messages.ToArray());
    }

    public async Task<(bool result, string[] message)> UpdateOrder(Order order)
    {
        var messages = new List<string>();
        var existing = _context.Orders.First(o => o.Id == order.Id);
        if (existing == null)
        {
            messages.Add($"No order with id '{order.Id}' found");
            return (false, messages.ToArray());
        }
        var validation = ValidateOrder(order, existing);
        if (!validation.valid)
        {
            return (false, messages.ToArray());
        }
        existing.CustomerName = order.CustomerName;
        existing.Status = order.Status;
        existing.LastUpdated = DateTime.UtcNow;
        //Update exisiting and insert new order items.
        foreach (var orderItem in order.OrderItems)
        {
            var existingOrderItem = existing.OrderItems.First(o => o.InventoryItemId == orderItem.InventoryItemId);
            if (existingOrderItem == null)
            {
                AddItem(existing, orderItem);
            }
            else
            {
                UpdateItem(orderItem, existingOrderItem);
            }
        }
        //Handle remove items
        var itemsToRemove = new List<OrderItem>();
        foreach (var orderItem in order.OrderItems)
        {
            if (existing.OrderItems.Any(o => o.InventoryItemId == orderItem.InventoryItemId)) continue;
            RemoveItem(existing, orderItem.InventoryItem);
        }
        _context.Update(existing);
        await _context.SaveChangesAsync();
        return (true, messages.ToArray());
    }

    public async Task<(bool result, string message)> RemoveOrder(int id)
    {

        var existing = _context.Orders.First(o => o.Id == id);
        if (existing == null)
        {
            return (false, $"No order with id '{id}' found");
        }
        UpdateInventory(existing.OrderItems);
        _context.Remove(existing);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }

    private (bool valid, string message) ValidateOrder(Order order, Order? orderToUpate)
    {
        bool update = orderToUpate != null;
        if (!update && order.Id > 0 && _context.Orders.Any(o => o.Id == order.Id))
        {
            return (false, message: $"Order '{order.Id}' alread exists");
        }
        if (order.OrderItems.Count == 0)
        {
            return (false, "Order must have at least one order item");
        }
        var itemsValid = ValidateOrderItems(order.OrderItems);
        if (!itemsValid.valid)
        {
            return (false, itemsValid.message);
        }
        return (true, string.Empty);
    }

    private (bool valid, string message) ValidateOrderItems(List<OrderItem> orderItems)
    {
        foreach (var orderItem in orderItems)
        {
            if (orderItem.InventoryItem == null && orderItem.InventoryItemId > 0)
            {
                orderItem.InventoryItem = _context.InventoryItems.FirstOrDefault(ii => ii.Id == orderItem.InventoryItemId);
            }
            if (orderItem.InventoryItem == null)
            {
                return (false, $"Inventory item with id '{orderItem.InventoryItemId}' does not exist");
            }
            if (orderItem.InventoryItem.Quantity - orderItem.OrderedQuantity < 0)
            {
                return (false, $"Only '{orderItem.InventoryItem.Quantity}' of {orderItem.InventoryItem.Name} left in stock");
            }
        }
        return (true, string.Empty);
    }

    private void UpdateInventory(InventoryItem inventoryItem, int quantity)
    {
        inventoryItem.Quantity -= quantity;
        _context.InventoryItems.Update(inventoryItem);
    }

    private void UpdateInventory(List<OrderItem> orderItems)
    {
        foreach (var item in orderItems)
        {
            item.InventoryItem.Quantity -= item.OrderedQuantity;
            _context.InventoryItems.Update(item.InventoryItem);
        }
    }

    private void UpdateOrderedQuantity(OrderItem orderItem, int quantity)
    {

        orderItem.OrderedQuantity += quantity;
        orderItem.InventoryItem.Quantity -= quantity;
    }

    public void AddItem(Order order, OrderItem orderItem)
    {
        var newOrderItem = new OrderItem { OrderId = order.Id, InventoryItemId = orderItem.Id, InventoryItem = orderItem.InventoryItem, OrderedQuantity = orderItem.OrderedQuantity };
        UpdateInventory(newOrderItem.InventoryItem, newOrderItem.OrderedQuantity);
        order.OrderItems.Add(newOrderItem);
    }

    public void UpdateItem(OrderItem orderItem, OrderItem existing)
    {
        var quantityChanged = orderItem.OrderedQuantity - existing.OrderedQuantity;
        existing.OrderedQuantity += quantityChanged;
        _context.OrderItems.Update(existing);
        UpdateInventory(orderItem.InventoryItem, quantityChanged);
    }

    private void RemoveItem(Order order, InventoryItem item)
    {
        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.InventoryItemId == item.Id);
        if (orderItem != null)
        {
            order.OrderItems.Remove(orderItem);
            UpdateInventory(orderItem.InventoryItem, orderItem.OrderedQuantity * -1);
        }
    }
}
