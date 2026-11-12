using Inventory_API.DTOs;
using Inventory_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_API.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryMovementsController : ControllerBase
{
    private readonly InventoryMovementService _service;

    public InventoryMovementsController(InventoryMovementService service)
    {
        _service = service;
    }

    [HttpPost("entries")]
    public IActionResult AddEntry(InventoryEntryDto dto)
    {
        var result = _service.AddEntry(dto);

        if (result == 1)
        {
            return NotFound("The specified product was not found.");
        }

        return Ok(new
        {
            message = "Inventory entry registered successfully.",
        });
    }

    [HttpPost("exits")]
    public IActionResult AddExit(InventoryExitDto dto)
    {
        var result = _service.AddExit(dto);

        if (result == 1)
        {
            return NotFound("The specified product was not found.");
        }

        if (result == 2)
        {
            return BadRequest("Insufficient stock for this operation.");
        }

        return Ok(new
        {
            message = "Inventory exit registered successfully"
        });
    }

    [HttpGet("movements")]
    public IActionResult GetMovements()
    {
        var movements = _service.GetAll();
        return Ok(movements);
    }

    [HttpGet("products/{productId}/movements")]
    public IActionResult GetMovementsByProduct(int productId)
    {
        if (productId <= 0)
        {
            return BadRequest("ProductId must be greater than zero.");
        }
        
        if(!_service.ProductExist(productId))
        {
            return NotFound("The specified product was not found.");
        }

        var movements = _service.GetByProductId(productId);

        return Ok(movements);
    }

}