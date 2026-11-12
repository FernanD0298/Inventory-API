using Inventory_API.Data;
using Inventory_API.DTOs;
using Inventory_API.Models;
using Inventory_API.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Tests;

public class InventoryMovementServiceTests
{
    private Product CreateTestProduct(InventoryDbContext context, int stock)
    {
        var category = new Category
        {
            Name = "Test Category"
        };

        context.Categories.Add(category);
        context.SaveChanges();

        var product = new Product
        {
            Name = "Test Product",
            Stock = stock,
            CategoryId = category.Id
        };

        context.Products.Add(product);
        context.SaveChanges();

        return product;
    }

    private InventoryDbContext CreateTestContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>().UseSqlite(connection).Options;
        var context = new InventoryDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public void AddExit_WhenStockIsInsufficient_ReturnsErrorCode()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateTestContext(connection);

        var product = CreateTestProduct(context, 3);

        var service = new InventoryMovementService(context);

        var result = service.AddExit(new InventoryExitDto
        {
            ProductId = product.Id,
            Quantity = 5
        });

        Assert.Equal(2, result);
        Assert.Equal(3, context.Products.Find(product.Id)!.Stock);
        Assert.Empty(context.InventoryMovements.ToList());
    }

    [Fact]
    public void AddExit_WhenStockIsInsufficient_UpdatesStockAndRecordsMovement()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateTestContext(connection);

        var product = CreateTestProduct(context, 10);

        var service = new InventoryMovementService(context);

        var result = service.AddExit(new InventoryExitDto
        {
            ProductId = product.Id,
            Quantity = 4
        });

        Assert.Equal(0, result);
        Assert.Equal(6, context.Products.Find(product.Id)!.Stock);

        var movement = Assert.Single(context.InventoryMovements.ToList());
        Assert.Equal(product.Id, movement.ProductId);
        Assert.Equal(4, movement.Quantity);
        Assert.Equal("Exit", movement.Type);
    }

    [Fact]
    public void AddEntry_WhenProductExists_UpdateStockAndRecordsMovement()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateTestContext(connection);

        var product = CreateTestProduct(context, 5);

        var service = new InventoryMovementService(context);

        var result = service.AddEntry(new InventoryEntryDto
        {
            ProductId = product.Id,
            Quantity = 4
        });

        Assert.Equal(0, result);
        Assert.Equal(9, context.Products.Find(product.Id)!.Stock);

        var movement = Assert.Single(context.InventoryMovements.ToList());
        Assert.Equal(product.Id, movement.ProductId);
        Assert.Equal(4, movement.Quantity);
        Assert.Equal("Entry", movement.Type);
    }

    [Fact]
    public void AddEntry_WhenProductDoesNotExist_ReturnsErrorCode()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateTestContext(connection);

        var service = new InventoryMovementService(context);

        var result = service.AddEntry(new InventoryEntryDto
        {
            ProductId = 999,
            Quantity = 4
        });

        Assert.Equal(1, result);
        Assert.Empty(context.InventoryMovements.ToList());
    }

    [Fact]
    public void AddExit_WhenProductDoesNotExist_ReturnsErrorCode()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateTestContext(connection);

        var service = new InventoryMovementService(context);

        var result = service.AddExit(new InventoryExitDto
        {
            ProductId = 999,
            Quantity = 4
        });

        Assert.Equal(1, result);
        Assert.Empty(context.InventoryMovements.ToList());
    }

    [Fact]
    public void GetAll_WhenMovementsExist_ReturnsAllMovements()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateTestContext(connection);

        var product = CreateTestProduct(context, 10);
        var service = new InventoryMovementService(context);

        service.AddEntry(new InventoryEntryDto
        {
            ProductId = product.Id,
            Quantity = 4
        });

        service.AddExit(new InventoryExitDto
        {
            ProductId = product.Id,
            Quantity = 3
        });

        var movements = service.GetAll();

        Assert.Equal(2, movements.Count);
        Assert.Contains(movements, m => m.Type == "Entry" && m.Quantity == 4);
        Assert.Contains(movements, m => m.Type == "Exit" && m.Quantity == 3);
    }

    [Fact]
    public void GetProductById_WhenMultipleProductsExist_ReturnsOnlyMatchingMovements()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateTestContext(connection);

        var product1 = CreateTestProduct(context, 10);
        var product2 = CreateTestProduct(context, 20);

        var service = new InventoryMovementService(context);

        service.AddEntry(new InventoryEntryDto
        {
            ProductId = product1.Id,
            Quantity = 4
        });

        service.AddExit(new InventoryExitDto
        {
            ProductId = product2.Id,
            Quantity = 3
        });

        service.AddEntry(new InventoryEntryDto
        {
            ProductId = product1.Id,
            Quantity = 2
        });

        var movements = service.GetByProductId(product1.Id).ToList();

        Assert.Equal(2, movements.Count);
        Assert.All(movements, m => Assert.Equal(product1.Id, m.ProductId));
        Assert.Contains(movements, m => m.Type == "Entry" && m.Quantity == 4);
        Assert.Contains(movements, m => m.Type == "Entry" && m.Quantity == 2);
    }

    [Fact]
    public void GetAll_WhenNoMovementsExist_ReturnsEmptyCollection()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateTestContext(connection);

        var service = new InventoryMovementService(context);

        var movements = service.GetAll();

        Assert.NotNull(movements);
        Assert.Empty(movements);
    }

    [Fact]
    public void GetByProductId_WhenProductHasNoMovements_ReturnsEmptyCollection()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        using var context = CreateTestContext(connection);
        
        var product = CreateTestProduct(context, 10);
        var service = new InventoryMovementService(context);
        
        var movements = service.GetByProductId(product.Id).ToList();
        
        Assert.Empty(movements);
    }

}
