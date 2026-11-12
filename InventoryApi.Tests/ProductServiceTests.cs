using Inventory_API.Data;
using Inventory_API.DTOs;
using Inventory_API.Models;
using Inventory_API.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Tests;

public class ProductServiceTests
{

    private Category CreateTestCategory(InventoryDbContext context)
    {
        var category = new Category
        {
            Name = "Test Category"
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
    public void Create_WhenCategoryExists_ReturnsCreatedProduct()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var context = CreateTestContext(connection);

        var category = CreateTestCategory(context);

        var service = new ProductService(context);

        var dto = new CreateProductDto
        {
            Name = "Test Product",
            Stock = 10,
            CategoryId = category.Id
        };

        var result = service.Create(dto);

        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(10, result.Stock);
        Assert.Equal(category.Id, result.CategoryId);

        var savedProduct = context.Products.Find(result.Id);

        Assert.NotNull(savedProduct);
        Assert.Equal("Test Product", savedProduct.Name);
    }

    [Fact]
    public void Create_WhenCategoryDoesNotExist_ReturnsNull()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var context = CreateTestContext(connection);

        var service = new ProductService(context);

        var dto = new CreateProductDto
        {
            Name = "Test Product",
            Stock = 10,
            CategoryId = 999
        };

        var result = service.Create(dto);

        Assert.Null(result);
        Assert.Empty(context.Products);
    }

    [Fact]
    public void Update_WhenProductExists_UpdatesProduct()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var context = CreateTestContext(connection);

        var category = CreateTestCategory(context);

        var product = new Product
        {
            Name = "Old Product",
            Stock = 5,
            CategoryId = category.Id
        };

        context.Products.Add(product);
        context.SaveChanges();

        var service = new ProductService(context);

        var dto = new UpdateProductDto
        {
            Name = "Update Product",
            Stock = 20,
            CategoryId = category.Id
        };

        var result = service.Update(product.Id, dto);

        Assert.NotNull(result);
        Assert.Equal("Update Product", result.Name);
        Assert.Equal(20, result.Stock);
        Assert.Equal(category.Id, result.CategoryId);

        var savedProduct = context.Products.Find(product.Id);

        Assert.NotNull(savedProduct);
        Assert.Equal("Update Product", savedProduct.Name);
        Assert.Equal(20, savedProduct.Stock);
    }

    [Fact]
    public void Update_WhenProductDoesNotExist_ReturnsNull()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var context = CreateTestContext(connection);

        var service = new ProductService(context);

        var dto = new UpdateProductDto
        {
            Name = "Update Product",
            Stock = 20,
            CategoryId = 1
        };

        var result = service.Update(999, dto);

        Assert.Null(result);
        Assert.Empty(context.Products);
    }

    [Fact]
    public void Update_WhenCategoryDoesNotExist_TrowsArgumentException()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var context = CreateTestContext(connection);

        var category = CreateTestCategory(context);
        var product = CreateTestProduct(context, category, 5, "Test Product");

        var service = new ProductService(context);

        var dto = new UpdateProductDto
        {
            Name = "Update Product",
            Stock = 20,
            CategoryId = 999
        };

        Assert.Throws<ArgumentException>(() => service.Update(product.Id, dto));

        var savedProduct = context.Products.Find(product.Id);

        Assert.NotNull(savedProduct);
        Assert.Equal("Test Product", savedProduct.Name);
        Assert.Equal(5, savedProduct.Stock);
        Assert.Equal(category.Id, savedProduct.CategoryId);
    }

    [Fact]
    public void Delete_WhenProductExists_RemovesProduct()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context);
        var product = CreateTestProduct(context, category, 10, "Test Product");
        
        var productId = product.Id;
        var service = new ProductService(context);
        
        var result = service.Delete(productId);
        
        Assert.True(result);
        Assert.Null(context.Products.Find(productId));
    }

    [Fact]
    public void Delete_WhenProductDoesNotExist_ReturnsFalse()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new ProductService(context);
        
        var result = service.Delete(999);
        
        Assert.False(result);
        Assert.Empty(context.Products);
    }

    [Fact]
    public void GetAll_WhenProductExists_ReturnsAllProducts()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context);
        var product1 = CreateTestProduct(context, category, 10, "Keyboard");
        var product2 = CreateTestProduct(context, category, 15, "Mouse");
        
        var service = new ProductService(context);
        
        var result = service.GetAll();
        
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Keyboard");
        Assert.Contains(result, p => p.Name == "Mouse");
    }

    [Fact]
    public void GetAll_WhenProductDoesNotExist_ReturnsEmptyCollection()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new ProductService(context);
        
        var result = service.GetAll();
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetById_WhenProductExists_ReturnsProduct()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context);
        var product = CreateTestProduct(context, category, 10, "Keyboard");
        
        var service = new ProductService(context);
        
        var result = service.GetById(product.Id);
        
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Keyboard", result.Name);
        Assert.Equal(10, result.Stock);
    }

    [Fact]
    public void GetById_WhenProductDoesNotExist_ReturnsNull()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new ProductService(context);
        
        var result = service.GetById(999);
        
        Assert.Null(result);
    }

    [Fact]
    public void Update_WhenCategoryChanges_UpdateProductCategory()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);

        var category1 = new Category
        {
            Name = "Electronics"
        };

        var category2 = new Category
        {
            Name = "Accessories"
        };
        
        context.Categories.AddRange(category1, category2);
        context.SaveChanges();
        
        var product = CreateTestProduct(context, category1, 10, "Keyboard");
        
        var service = new ProductService(context);

        var dto = new UpdateProductDto
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = category2.Id
        };
        
        var result = service.Update(product.Id, dto);
        
        Assert.NotNull(result);
        Assert.Equal(category2.Id, result.CategoryId);
        
        var updatedProduct = context.Products.Find(product.Id);
        
        Assert.NotNull(updatedProduct);
        Assert.Equal(category2.Id, updatedProduct.CategoryId);
    }
}