using LogiTrack.Models;
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
    /// <summary>
    /// Gets a list of inventory items.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetItems()
    {
        // Your logic to get inventory items
        return Ok();
    }

    /// <summary>
    /// Gets a specific inventory item by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetItem(int id)
    {
        // Your logic to get a specific inventory item by ID
        return Ok();
    }

    /// <summary>
    /// Creates a new inventory item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult CreateItem([FromBody] InventoryItem item)
    {
        // Your logic to create a new inventory item
        return CreatedAtAction(nameof(GetItems), new { id = item.Id }, item);
    }
    /// <summary>
    /// Creates multiple new inventory items in a batch.
    /// </summary>
    /// <param name="items">An array of inventory items to be created.</param>
   
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult CreateItems([FromBody] InventoryItem[] items)
    {
        if (items == null || items.Length == 0)
        {
            return BadRequest("No items provided for creation.");
        }
        // Your logic to create new inventory items
        // Returning Ok with the created items, as CreatedAtAction is typically for single resources
        return Ok(items);
    }

    /// <summary>
    /// Updates an existing inventory item.
    /// </summary> 
    /// <param name="id">The ID of the inventory item to update.</param>
    /// <param name="item">The updated inventory item data.</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult UpdateItem(int id, [FromBody] InventoryItem item)
    {
        // Your logic to update the inventory item
        return NoContent();
    }

    /// <summary>
    /// Deletes an inventory item by ID.
    /// </summary>
    /// <param name="id">The ID of the inventory item to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult DeleteItem(int id)
    {
        // Your logic to delete the inventory item
        return NoContent();
    }
}
