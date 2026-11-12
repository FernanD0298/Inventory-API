using Inventory_API.DTOs;
using Inventory_API.Models;
using Inventory_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public IActionResult GetProducts()
    {
        var result = _productService.GetAll();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public IActionResult GetProductById(int id)
    {
        var product = _productService.GetById(id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public IActionResult CreateProduct([FromBody] CreateProductDto dto)
    {
        var product = _productService.Create(dto);

        if (product == null)
        {
            return BadRequest("The specified category does not exist");
        }
        
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var product = _productService.Update(id, dto);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        bool deleted = _productService.Delete(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

}