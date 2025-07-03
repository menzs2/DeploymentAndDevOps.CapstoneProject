using Microsoft.AspNetCore.Mvc;
using LogiTrack.Models;

namespace LogiTrack.Controllers;

/// <summary>
/// Controller for managing orders.
/// Provides endpoints to create, read, update, and delete orders.
/// </summary>

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status200OK)]
public class OrderController : ControllerBase
{
    private readonly LogiTrackContext _context;
    private readonly OrderService _service;
    public OrderController(LogiTrackContext context,  OrderService orderService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _service = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }


    /// <summary>
    /// Gets a list of orders.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetOrders()
    {
        var orders = _service.GetOrders();
        return orders.Any() ? Ok(orders) : NotFound();
    }

    /// <summary>
    /// Gets a specific order by ID.
    /// </summary>
    /// <param name="id">The ID of the order to retrieve.</param>
    /// <returns>An IActionResult containing the order details or a NotFound result.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetOrder(int id)
    {

        var order = _service.GetOrders().FirstOrDefault(i => i.Id == id);
        return order != null ? Ok(order) : NotFound($"No order with id '{id} found.");
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="order">The order to create.</param>
    /// <returns>An IActionResult indicating the result of the creation operation.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        if (order == null)
        {
            return BadRequest("No order provided for creation.");
        }
        var result = await _service.InsertOrder(order);
        if (!result.result)
        {
            return BadRequest(result.message);
        }
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    /// <summary>
    /// Creates multiple new orders in a batch.
    /// </summary>
    /// <param name="orders">The array of orders to create.</param>
    /// <returns>An IActionResult indicating the result of the creation operation.</returns>
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateOrders([FromBody] Order[] orders)
    {
        if (orders == null || orders.Length == 0)
        {
            return BadRequest("No orders provided for creation.");
        }
        var result = await _service.InsertOrder(orders.ToList());
        if (!result.result)
        {
            return BadRequest(result.message);
        }
        return Ok(orders);
    }

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="id">The ID of the order to update.</param>
    /// <param name="order">The updated order data.</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
    {
        var existing = _context.Orders.First(o => o.Id == id);
        if (existing == null)
        {
            return BadRequest($"No order with id '{id}' found");
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
        return NoContent();
    }

    /// <summary>
    /// Deletes an order by ID.
    /// </summary>
    /// <param name="id">The ID of the order to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var existing = _context.Orders.Where(o => o.Id == id);
        if (existing == null)
        {
            return BadRequest($"No order with id '{id}' found");
        }
        _context.Remove(existing);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
