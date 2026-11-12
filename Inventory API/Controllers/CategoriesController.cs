using Inventory_API.DTOs;
using Inventory_API.Models;
using Inventory_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    public IActionResult CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var category = _categoryService.Create(dto);
        
        return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
    }
    
    [HttpGet]
    public IActionResult GetCategories()
    {
        var categories = _categoryService.GetAll();
        
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public IActionResult GetCategoryById(int id)
    {
        var category = _categoryService.GetById(id);
        
        if(category == null)
        {
            return NotFound();
        }
        
        return Ok(category);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        var category = _categoryService.Update(id, dto);
        
        if (category == null)
        {
            return NotFound();
        }
        
        return Ok(category);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteCategory(int id, DeleteCategoryDto dto)
    {
        var result = _categoryService.ReassignProductsAndDelete(id, dto.TargetCategoryId);

        switch (result)
        {
            case 1:
                return BadRequest("The target category must be different from the category being deleted.");
            case 2:
                return NotFound("The category to delete was not found.");
            case 3:
                return NotFound("The target category was not found.");
        }

        return NoContent();
    }
}