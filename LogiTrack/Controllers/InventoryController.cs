using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _inMemoryStoreCache;

    public InventoryController(LogiTrackContext dbContext, IMemoryCache inMemoryStoreCache)
    {
        _context = dbContext;
        _inMemoryStoreCache = inMemoryStoreCache;
    }

    /// <summary>
    /// Gets a list of inventory items.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetItems()
    {
        // Check if the items are cached
        if (_inMemoryStoreCache.TryGetValue("inventoryItems", out List<InventoryItem>? cachedItems))
        {
            return Ok(cachedItems);
        }
        var items = _context.InventoryItems.ToList();
        // If not cached, store the items in cache
        _inMemoryStoreCache.Set("inventoryItems", items, TimeSpan.FromMinutes(5));
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
        if (_inMemoryStoreCache.TryGetValue($"inventoryItem_{id}", out InventoryItem? cachedItem))
        {
            return Ok(cachedItem);
        }
        var item = _context.InventoryItems.Where(i => i.Id == id).FirstOrDefault();
        return item != null ? Ok(item) : NotFound($"No item with id '{id} found.");
    }

    /// <summary>
    /// Creates a new inventory item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    //[Authorize(Roles = "Admin,Manager")]
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
    public async Task<IActionResult> CreateItems([FromBody] InventoryItem[] items)
    {
        //validate authentication and authorization
        if (User.Identity == null || !User.Identity.IsAuthenticated || !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
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
    //[Authorize(Roles = "Admin,Manager")]
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
