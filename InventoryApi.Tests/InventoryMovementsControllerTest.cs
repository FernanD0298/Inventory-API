using Inventory_API.Controllers;
using Inventory_API.Data;
using Inventory_API.DTOs;
using Inventory_API.Models;
using Inventory_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Tests;

public class InventoryMovementsControllerTest
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
    public void AddEntry_WhenProductExists_ReturnOk()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateTestContext(connection);

        var product = CreateTestProduct(context, 5);

        var service = new InventoryMovementService(context);
        var controller = new InventoryMovementsController(service);

        var dto = new InventoryEntryDto
        {
            ProductId = product.Id,
            Quantity = 4
        };
        
        var result = controller.AddEntry(dto);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(9, context.Products.Find(product.Id)!.Stock);
    }

    [Fact]
    public void AddEntry_WhenProductDoesNotExists_ReturnNotFound()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateTestContext(connection);

        var service = new InventoryMovementService(context);
        var controller = new InventoryMovementsController(service);

        var dto = new InventoryEntryDto
        {
            ProductId = 999,
            Quantity = 4
        };
        
        var result = controller.AddEntry(dto);

        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Empty(context.InventoryMovements);
    }

    [Fact]
    public void AddExit_WhenStockIsInsufficient_ReturnsBadRequest()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        using var context = CreateTestContext(connection);

        var product = CreateTestProduct(context, 3);
        
        var service = new InventoryMovementService(context);
        var controller = new InventoryMovementsController(service);

        var dto = new InventoryExitDto
        {
            ProductId = product.Id,
            Quantity = 5
        };
        
        var result = controller.AddExit(dto);
        
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(3, context.Products.Find(product.Id)!.Stock);
        Assert.Empty(context.InventoryMovements);
    }

    [Fact]
    public void AddExit_WhenStockIsSufficient_ReturnsOk()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        using var context = CreateTestContext(connection);
        
        var product = CreateTestProduct(context, 10);
        
        var service = new InventoryMovementService(context);
        var controller = new InventoryMovementsController(service);

        var dto = new InventoryExitDto
        {
            ProductId = product.Id,
            Quantity = 4
        };

        var result = controller.AddExit(dto);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(6, context.Products.Find(product.Id)!.Stock);

        var movement = Assert.Single(context.InventoryMovements);
        Assert.Equal(product.Id, movement.ProductId);
        Assert.Equal(4, movement.Quantity);
        Assert.Equal("Exit", movement.Type);
    }

    [Fact]
    public void GetMovements_WhenNoMovementsExist_ReturnsEmptyCollection()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        using var context = CreateTestContext(connection);
        
        var service = new InventoryMovementService(context);
        var controller = new InventoryMovementsController(service);
        
        var result = controller.GetMovements();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var movements = Assert.IsAssignableFrom<System.Collections.IEnumerable>(okResult.Value);
        
        Assert.Empty(movements.Cast<object>());
    }

    [Fact]
    public void GetMovements_WhenMovementExists_ReturnsAllMovements()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        using var context = CreateTestContext(connection);

        var product = CreateTestProduct(context, 10);
        
        var service = new InventoryMovementService(context);
        var controller = new InventoryMovementsController(service);

        service.AddEntry(new InventoryEntryDto
        {
            ProductId = product.Id,
            Quantity = 4
        });

        service.AddExit(new InventoryExitDto
        {
            ProductId = product.Id,
            Quantity = 2
        });
        
        var result = controller.GetMovements();
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var movements = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<InventoryMovement>>(okResult.Value);

        var movementList = movements.ToList();
        
        Assert.Equal(2, movementList.Count);
        Assert.Contains(movementList, m => m.Type == "Entry" && m.Quantity == 4);
        Assert.Contains(movementList, m => m.Type == "Exit" && m.Quantity == 2);
    }
    
    [Fact]
    public void GetMovementsByProduct_WhenProductExists_ReturnsOnlyItsMovements()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        using var context = CreateTestContext(connection);
        
        var product1 = CreateTestProduct(context, 10);
        var product2 = CreateTestProduct(context, 10);
        
        var service = new InventoryMovementService(context);
        var controller = new InventoryMovementsController(service);

        service.AddEntry(new InventoryEntryDto
        {
            ProductId = product1.Id,
            Quantity = 4
        });

        service.AddEntry(new InventoryEntryDto
        {
            ProductId = product2.Id,
            Quantity = 3
        });

        service.AddExit(new InventoryExitDto
        {
            ProductId = product1.Id,
            Quantity = 2
        });
        
        var result = controller.GetMovementsByProduct(product1.Id);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var movements = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<InventoryMovement>>(okResult.Value);
        
        var movementList = movements.ToList();
        
        Assert.Equal(2, movementList.Count);
        Assert.All(movementList, movement => Assert.Equal(product1.Id, movement.ProductId));
    }

    [Fact]
    public void GetMovementsByProduct_WhenProductDoesNotExist_ReturnsNotFound()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        using var context = CreateTestContext(connection);
        
        var service = new InventoryMovementService(context);
        var controller = new InventoryMovementsController(service);
        
        var result = controller.GetMovementsByProduct(999);
        
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetMovementsByProduct_WhenProductIdIsZero_ReturnsBadRequest()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        using var context = CreateTestContext(connection);
        
        var service = new InventoryMovementService(context);
        var controller = new InventoryMovementsController(service);
        
        var result = controller.GetMovementsByProduct(0);
        
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void AddExit_WhenProductDoesNotExist_ReturnsNotFound()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        using var context = CreateTestContext(connection);
        
        var service = new InventoryMovementService(context);
        var controller = new InventoryMovementsController(service);

        var dto = new InventoryExitDto
        {
            ProductId = 999,
            Quantity = 4
        };
        
        var result = controller.AddExit(dto);
        
        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Empty(context.InventoryMovements);
    }

    [Fact]
    public void GetMovementsByProduct_WhenProductIdIsNegative_ReturnsBadRequest()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        using var context = CreateTestContext(connection);
        
        var service = new InventoryMovementService(context);
        var controller = new InventoryMovementsController(service);
        
        var result = controller.GetMovementsByProduct(-1);
        
        Assert.IsType<BadRequestObjectResult>(result);
    }
    
}