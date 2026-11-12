using System.ComponentModel.DataAnnotations;

namespace Inventory_API.DTOs;

public class UpdateProductDto
{
    [Required(ErrorMessage = "Product name is required.")]
    [MinLength(2, ErrorMessage = "Name must have at least 2 characters.")]
    public string Name { get; set; } = string.Empty;
    
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    public int Stock { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than zero.")]
    public int CategoryId { get; set; }
}