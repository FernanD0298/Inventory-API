using System.ComponentModel.DataAnnotations;

namespace Inventory_API.DTOs;

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [MinLength(2, ErrorMessage = "Name must have at least 2 characters.")]
    public string Name { get; set; } = string.Empty;
}