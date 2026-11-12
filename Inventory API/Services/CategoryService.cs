using Inventory_API.Data;
using Inventory_API.DTOs;
using Inventory_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory_API.Services;

public class CategoryService
{
    private readonly InventoryDbContext _context;
    
    public CategoryService(InventoryDbContext context)
    {
        _context = context;
    }

    public Category Create(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name
        };

        _context.Categories.Add(category);
        _context.SaveChanges();
        
        return category;
    }
    
    public List<Category> GetAll()
    {
        return _context.Categories.ToList();
    }

    public CategoryDetailsDto? GetById(int id)
    {
        var category = _context.Categories.Include(c => c.Products).FirstOrDefault(c => c.Id == id);
        
        if (category == null)
        {
            return null;
        }

        return new CategoryDetailsDto
        {
            Id = category.Id,
            Name = category.Name,

            Products = category.Products.Select(product => new ProductSummaryDto()
            {
                Id = product.Id,
                Name = product.Name,
                Stock = product.Stock
            }).ToList()
        };
    }

    public Category? Update(int id, UpdateCategoryDto dto)
    {
        var category = _context.Categories.Find(id);

        if (category == null)
        {
            return null;
        }
        
        category.Name = dto.Name;
        _context.SaveChanges();
        
        return category;
    }

    public int ReassignProductsAndDelete(int categoryId, int targetCategoryId)
    {
        if (categoryId == targetCategoryId)
        {
            return 1;
        }
        
        var category = _context.Categories.Find(categoryId);

        if (category == null)
        {
            return 2;
        }

        var targetCategory = _context.Categories.Find(targetCategoryId);

        if (targetCategory == null)
        {
            return 3;
        }

        var products = _context.Products.Where(p => p.CategoryId == categoryId).ToList();

        foreach (var product in products)
        {
            product.CategoryId = targetCategoryId;
        }

        _context.Categories.Remove(category);
        _context.SaveChanges();
        
        return 0;
    }
}