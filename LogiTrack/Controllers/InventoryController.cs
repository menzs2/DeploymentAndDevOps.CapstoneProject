using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack.Controllers;

/// <summary>
/// Controller for managing inventory items.
/// Provides endpoints to create, read, update, and delete inventory items.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status200OK)]
public class InventoryController : ControllerBase
{
    private readonly LogiTrackContext _context;

    public InventoryController(LogiTrackContext dbContext)
    {
        _context = dbContext;
    }

    /// <summary>
    /// Gets a list of inventory items.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetItems()
    {
        var items = _context.InventoryItems.ToList<InventoryItem>();

        return items.Any() ? Ok(items) : NotFound();
    }

    /// <summary>
    /// Gets a specific inventory item by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetItem(int id)
    {

        var item = _context.InventoryItems.Where(i => i.Id == id);
        return item != null ? Ok(item) : NotFound($"No item with id '{id} found.");
    }

    /// <summary>
    /// Creates a new inventory item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateItem([FromBody] InventoryItem item)
    {
        if (item == null)
        {
            return BadRequest($"No item provided for creation.");
        }
        _context.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetItems), new { id = item.Id }, item);
    }
    /// <summary>
    /// Creates multiple new inventory items in a batch.
    /// </summary>
    /// <param name="items">An array of inventory items to be created.</param>

    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateItems([FromBody] InventoryItem[] items)
    {
        if (items == null || items.Length == 0)
        {
            return BadRequest("No items provided for creation.");
        }
        _context.InventoryItems.AddRange(items);
        await _context.SaveChangesAsync();
        return Ok(items);
    }

    /// <summary>
    /// Updates an existing inventory item.
    /// </summary> 
    /// <param name="id">The ID of the inventory item to update.</param>
    /// <param name="item">The updated inventory item data.</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] InventoryItem item)
    {
        if (item == null)
        {
            return BadRequest($"No item provided for creation.");
        }
        var existing = _context.InventoryItems.First(i => i.Id == id);
        existing.Name = item.Name;
        existing.Description = item.Description;
        existing.Quantity = item.Quantity;
        existing.Price = item.Price;
        existing.Location = item.Location;
        existing.LastUpdated = DateTime.UtcNow;
        _context.Update(existing);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes an inventory item by ID.
    /// </summary>
    /// <param name="id">The ID of the inventory item to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var existing = _context.InventoryItems.Where(i => i.Id == id);
        if (existing == null)
        {
            return BadRequest($"No item with id '{id}' found.");
        }
        _context.Remove(existing);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
