namespace Inventory_API.DTOs;

public class CategoryDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ProductSummaryDto> Products { get; set; } = new();
}