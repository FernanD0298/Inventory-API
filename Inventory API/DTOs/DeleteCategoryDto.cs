using System.ComponentModel.DataAnnotations;

namespace Inventory_API.DTOs;

public class DeleteCategoryDto
{
    [Range(1, int.MaxValue, ErrorMessage = "TargetCategoryId must be greater than zero.")]
    public int TargetCategoryId { get; set; }
}