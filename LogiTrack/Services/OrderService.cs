using LogiTrack.Models;
using Xunit.Sdk;

namespace LogiTrack;

public class OrderService
{
    private readonly LogiTrackContext _context;

    public OrderService(LogiTrackContext context)
    {
        _context = context;
    }

    public List<Order> GetOrders()
    {
        return _context.Orders.ToList();
    }

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
            var result = ValidateOrder(order, null);
            if (result.valid)
            {
                _context.Add(order);
                UpdateInventory(order.OrderItems);
            }
            else
            {
                messages.Add(result.message);
            }
        }
        await _context.SaveChangesAsync();
        return (messages.Any(), messages.ToArray());
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
        _context.SaveChanges();
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

    public void RemoveItem(Order order, InventoryItem item)
    {
        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.InventoryItemId == item.Id);
        if (orderItem != null)
        {
            order.OrderItems.Remove(orderItem);
            UpdateInventory(new List<OrderItem> { orderItem });
        }
    }
}
