using Inventory_API.Data;
using Inventory_API.DTOs;
using Inventory_API.Models;


namespace Inventory_API.Services;

public class InventoryMovementService
{
    private readonly InventoryDbContext _context;

    public InventoryMovementService(InventoryDbContext context)
    {
        _context = context;
    }

    public int AddEntry(InventoryEntryDto dto)
    {
        var product = _context.Products.Find(dto.ProductId);

        if (product == null)
        {
            return 1;
        }

        product.Stock += dto.Quantity;

        var movement = new InventoryMovement
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            Type = "Entry",
            Date = DateTime.Now
        };

        _context.InventoryMovements.Add(movement);
        _context.SaveChanges();

        return 0;
    }

    public int AddExit(InventoryExitDto dto)
    {
        var product = _context.Products.Find(dto.ProductId);

        if (product == null)
        {
            return 1;
        }

        if (product.Stock < dto.Quantity)
        {
            return 2;
        }

        product.Stock -= dto.Quantity;

        var movement = new InventoryMovement
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            Type = "Exit",
            Date = DateTime.Now
        };

        _context.InventoryMovements.Add(movement);
        _context.SaveChanges();

        return 0;
    }
    
    public List<InventoryMovement> GetAll()
    {
        return _context.InventoryMovements.OrderByDescending(m => m.Date).ToList();
    }
    
    public List<InventoryMovement> GetByProductId(int productId)
    {
        return _context.InventoryMovements.Where(m => m.ProductId == productId).OrderByDescending(m => m.Date).ToList();
    }

    public bool ProductExist(int productId)
    {
        return _context.Products.Any(p => p.Id == productId);
    }
}