using Inventory_API.Data;
using Inventory_API.DTOs;
using Inventory_API.Models;

namespace Inventory_API.Services;

public class ProductService
{
    private readonly InventoryDbContext _context;
    public ProductService(InventoryDbContext context)
    {
        _context = context;
    }

    public Product? Create(CreateProductDto dto)
    {
        var categoryExist = _context.Categories.Any(c => c.Id == dto.CategoryId);
        
        if (!categoryExist)
        {
            return null;
        }
        
        var product = new Product
        {
            Name = dto.Name,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId
        };
        
        _context.Products.Add(product);
        _context.SaveChanges();
        
        return product;
    }

    public Product? Update(int id, UpdateProductDto dto)
    {
        var product = GetById(id);

        if (product == null)
        {
            return null;
        }

        var categoryExist = _context.Categories.Any(c => c.Id == dto.CategoryId);

        if (!categoryExist)
        {
            throw new ArgumentException("The specified category does not exist");
        }
        
        product.Name = dto.Name;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;

        _context.SaveChanges();

        return product;
    }

    public bool Delete(int id)
    {
        var product = GetById(id);

        if (product == null)
        {
            return false;
        }
        
        _context.Products.Remove(product);
        _context.SaveChanges();
        
        return true;
    }

    public List<Product> GetAll()
    {
        return _context.Products.ToList();
    }

    public Product? GetById(int id)
    {
        return _context.Products.Find(id);
    }
}