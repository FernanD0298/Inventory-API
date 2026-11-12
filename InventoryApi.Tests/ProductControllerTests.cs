using Inventory_API.Controllers;
using Inventory_API.Data;
using Inventory_API.DTOs;
using Inventory_API.Models;
using Inventory_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Tests;

public class ProductControllerTests
{
    private Category CreateTestCategory(InventoryDbContext context, string name)
    {
        var category = new Category
        {
            Name = name
        };

        context.Categories.Add(category);
        context.SaveChanges();

        return category;
    }

    private Product CreateTestProduct(InventoryDbContext context, Category category, int stock, string name)
    {
        var product = new Product
        {
            Name = name,
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
    public void GetProducts_WhenProductsExist_ReturnOkWithProducts()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var context = CreateTestContext(connection);

        var category = CreateTestCategory(context, "Electronics");
        var prodcut1 = CreateTestProduct(context, category, 10, "Keyboard");
        var prodcut2 = CreateTestProduct(context, category, 15, "Mouse");

        var service = new ProductService(context);
        var controller = new ProductsController(service);
        
        var result = controller.GetProducts();
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var products = Assert.IsType<List<Product>>(okResult.Value);
        
        Assert.Equal(2, products.Count);
        Assert.Contains(products, p => p.Name == "Keyboard");
        Assert.Contains(products, p => p.Name == "Mouse");
    }

    [Fact]
    public void GetProducts_WhenProductsNotExist_ReturnOkWithEmptyCollection()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new ProductService(context);
        var controller = new ProductsController(service);
        
        var result = controller.GetProducts();
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var products = Assert.IsType<List<Product>>(okResult.Value);
        
        Assert.Empty(products);
    }

    [Fact]
    public void GetProductById_WhenProductExists_ReturnOkWithProduct()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context, "Electronics");
        var product = CreateTestProduct(context, category, 10, "Keyboard");
        
        var service = new ProductService(context);
        var controller = new ProductsController(service);
        
        var result = controller.GetProductById(product.Id);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProduct = Assert.IsType<Product>(okResult.Value);
        
        Assert.Equal(product.Id, returnedProduct.Id);
        Assert.Equal("Keyboard", returnedProduct.Name);
        Assert.Equal(10, returnedProduct.Stock);
    }

    [Fact]
    public void GetProductById_WhenProductNotExists_ReturnNotFound()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new ProductService(context);
        var controller = new ProductsController(service);
        
        var result = controller.GetProductById(999);
        
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void CreateProduct_WhenCategoryExists_ReturnCreatedAtAction()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context, "Electronics");
        
        var service = new ProductService(context);
        var controller = new ProductsController(service);

        var dto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = category.Id
        };
        
        var result = controller.CreateProduct(dto);
        
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var createdProduct = Assert.IsType<Product>(createdResult.Value);
        
        Assert.Equal(nameof(ProductsController.GetProductById), createdResult.ActionName);
        Assert.Equal(createdProduct.Id, createdResult.RouteValues!["id"]);
        Assert.Equal("Keyboard", createdProduct.Name);
        Assert.Equal(10, createdProduct.Stock);

        Assert.NotNull(context.Products.Find(createdProduct.Id));
    }

    [Fact]
    public void CreateProduct_WhenCategoryDoesNotExists_ReturnBadRequest()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new ProductService(context);
        var controller = new ProductsController(service);

        var dto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = 999
        };
        
        var result = controller.CreateProduct(dto);
        
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        
        Assert.Equal("The specified category does not exist",  badRequestResult.Value);
        Assert.Empty(context.Products);
    }

    [Fact]
    public void UpdateProduct_WhenProductExists_ReturnsOkWithUpdatedProduct()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context, "Electronics");
        var product = CreateTestProduct(context, category, 5, "Old Keyboard");
        
        var service = new ProductService(context);
        var controller = new ProductsController(service);

        var dto = new UpdateProductDto
        {
            Name = "Update Keyboard",
            Stock = 20,
            CategoryId = category.Id
        };
        
        var result = controller.UpdateProduct(product.Id, dto);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedProduct = Assert.IsType<Product>(okResult.Value);
        
        Assert.Equal("Update Keyboard", updatedProduct.Name);
        Assert.Equal(20, updatedProduct.Stock);
        
        var savedProduct = context.Products.Find(product.Id);

        Assert.NotNull(savedProduct);
        Assert.Equal("Update Keyboard", savedProduct.Name);
        Assert.Equal(20, savedProduct.Stock);
    }

    [Fact]
    public void UpdateProduct_WhenProductDoesNotExists_ReturnNotFound()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new ProductService(context);
        var controller = new ProductsController(service);

        var dto = new UpdateProductDto
        {
            Name = "Update Keyboard",
            Stock = 20,
            CategoryId = 999
        };
        
        var result = controller.UpdateProduct(999, dto);
        
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void UpdateProduct_WhenCategoryDoesNotExists_ReturnBadRequest()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context, "Electronics");
        var product = CreateTestProduct(context, category, 10, "Keyboard");
        
        var service = new ProductService(context);
        var controller = new ProductsController(service);

        var dto = new UpdateProductDto
        {
            Name = "Update Keyboard",
            Stock = 20,
            CategoryId = 999
        };
        
        var result = controller.UpdateProduct(product.Id, dto);
        
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        
        Assert.Equal("The specified category does not exist", badRequestResult.Value);
        
        var savedProduct = context.Products.Find(product.Id);
        
        Assert.NotNull(savedProduct);
        Assert.Equal("Keyboard", savedProduct.Name);
        Assert.Equal(10, savedProduct.Stock);
        Assert.Equal(category.Id, savedProduct.CategoryId);
    }
    
    [Fact]
    public void DeleteProduct_WhenProductExists_ReturnsNotContent()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context, "Electronics");
        var product = CreateTestProduct(context, category, 10, "Keyboard");
        
        var productId = product.Id;
        
        var service = new ProductService(context);
        var controller = new ProductsController(service);
        
        var result = controller.DeleteProduct(productId);
        
        Assert.IsType<NoContentResult>(result);
        Assert.Null(context.Products.Find(productId));
    }

    [Fact]
    public void DeleteProduct_WhenProductDoesNotExists_ReturnNotFound()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new ProductService(context);
        var controller = new ProductsController(service);
        
        var result = controller.DeleteProduct(999);
        
        Assert.IsType<NotFoundResult>(result);
        Assert.Empty(context.Products);
    }
}