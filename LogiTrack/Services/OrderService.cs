using LogiTrack.Models;

namespace LogiTrack;

public class OrderService
{
    private readonly LogiTrackContext _context;

    public OrderService(LogiTrackContext context)
    {
        _context = context;
    }

    public List<Order> GetOrders() => _context.Orders.ToList();

    public async Task<(bool result, string message)> InsertOrder(Order order)
    {
        var validation = ValidateOrder(order, null);
        if (validation.valid)
        {
            _context.Add(order);
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
        await _context.SaveChangesAsync();
        return (messages.Any(), messages.ToArray());
    }

    public async Task<(bool result, string[] message)> UpdateOrder(Order order)
    {
        var messages = new List<string>();
        var existing = _context.Orders.First(o => o.Id == order.Id);
        if (existing == null)
        {
            return (false, new string[] { $"No order with id '{order.Id}' found" });
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
                existing.AddItem(orderItem.InventoryItem);
            }
            else
            {
                existingOrderItem.OrderedQuantity = orderItem.OrderedQuantity;
            }
        }
        //Handle remove items
        var itemsToRemove = new List<OrderItem>();
        foreach (var orderItem in order.OrderItems)
        {
            if (existing.OrderItems.Any(o => o.InventoryItemId == orderItem.InventoryItemId)) continue;
            existing.RemoveItem(orderItem.InventoryItem);
        }
        _context.Update(existing);
        await _context.SaveChangesAsync();
        return (true, messages.ToArray());
    }

    public async Task<(bool result, string message)> RemoveOrder(int id) {

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
        foreach (var orderItem in order.OrderItems)
        {

            if (ValidateOrderItem(orderItem, orderItem.OrderedQuantity))
            {
                return (false, message: $"Only '{orderItem.InventoryItem.Quantity}' of {orderItem.InventoryItem.Name} left in stock");
            }
        }
        return (true, string.Empty);
    }

    private bool ValidateOrderItem(OrderItem orderItem, int quantityChanged)
    {
        return orderItem.InventoryItem.Quantity - quantityChanged >= 0;
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

    public void AddItem(Order order, InventoryItem item, int quantity)
    {
        var newOrderItem = new OrderItem { OrderId = order.Id, InventoryItemId = item.Id, InventoryItem = item };
        UpdateInventory(new List<OrderItem> { newOrderItem });
        order.OrderItems.Add(newOrderItem);
    }

    private void RemoveItem(Order order, InventoryItem item)
    {
        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.InventoryItemId == item.Id);
        if (orderItem != null)
        {
            order.OrderItems.Remove(orderItem);
            UpdateInventory(new List<OrderItem> { orderItem });
        }
    }
}
