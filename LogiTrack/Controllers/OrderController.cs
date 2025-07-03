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
    private readonly OrderService _service;
    public OrderController(LogiTrackContext context,  OrderService orderService)
    {
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
        if (order == null )
        {
            return BadRequest("No order provided for creation.");
        }
        var result = await _service.UpdateOrder(order);
        if (!result.result)
        {
            return BadRequest(result.message);
        }
        return Ok(order);
        
    }

    /// <summary>
    /// Deletes an order by ID.
    /// </summary>
    /// <param name="id">The ID of the order to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        
        await _service.RemoveOrder(id);
        return NoContent();
    }
}
