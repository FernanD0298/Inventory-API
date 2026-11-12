using Inventory_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory_API.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }
    
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }
}