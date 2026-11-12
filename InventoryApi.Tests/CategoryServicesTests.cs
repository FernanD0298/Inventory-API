using Inventory_API.Data;
using Inventory_API.DTOs;
using Inventory_API.Models;
using Inventory_API.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Tests;

public class CategoryServicesTests
{
    private InventoryDbContext CreateTestContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>().UseSqlite(connection).Options;
        var context = new InventoryDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public void Create_WhenCalled_ReturnsCreateCategory()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);

        var service = new CategoryService(context);

        var dto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var result = service.Create(dto);
        
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Electronics", result.Name);
        
        var savedCategory = context.Categories.Find(result.Id);
        
        Assert.NotNull(savedCategory);
        Assert.Equal("Electronics", savedCategory.Name);
    }

    [Fact]
    public void GetAll_WhenCategoryExists_ReturnsAllCategories()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
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
        
        var service = new CategoryService(context);
        
        var result = service.GetAll();
        
        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.Name == "Accessories");
        Assert.Contains(result, c => c.Name == "Electronics");
    }

    [Fact]
    public void GetAll_WhenCategoryDoesNotExist_ReturnsEmptyCollection()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new CategoryService(context);
        
        var result = service.GetAll();
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetById_WhenCategoryExists_ReturnsCategoryWithProducts()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);

        var category = new Category
        {
            Name = "Electronics"
        };
        
        context.Categories.Add(category);
        context.SaveChanges();
        
        var product1 = new Product
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = category.Id
        };

        var product2 = new Product
        {
            Name = "Mouse",
            Stock = 15,
            CategoryId = category.Id
        };
        
        context.Products.AddRange(product1, product2);
        context.SaveChanges();
        
        var service = new CategoryService(context);
        
        var result = service.GetById(category.Id);
        
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal("Electronics", result.Name);
        
        Assert.Equal(2, result.Products.Count);
        Assert.Contains(result.Products, p => p.Name == "Keyboard");
        Assert.Contains(result.Products, p => p.Name == "Mouse");
    }

    [Fact]
    public void GetById_WhenCategoryHasNoProducts_ReturnsCategoryWithEmptyProducts()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);

        var category = new Category
        {
            Name = "Electronics"
        };
        
        context.Categories.Add(category);
        context.SaveChanges();
        
        var service = new CategoryService(context);
        
        var result = service.GetById(category.Id);
        
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal("Electronics", result.Name);
        Assert.NotNull(result.Products);
        Assert.Empty(result.Products);
    }
    
    [Fact]
    public void GetById_WhenCategoryDoesNotExist_ReturnsNull()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new CategoryService(context);
        
        var result = service.GetById(999);
        
        Assert.Null(result);
    }

    [Fact]
    public void Update_WhenCategoryExists_UpdatesCategoryName()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var context = CreateTestContext(connection);

        var category = new Category
        {
            Name = "Old Category"
        };

        context.Categories.Add(category);
        context.SaveChanges();

        var service = new CategoryService(context);

        var dto = new UpdateCategoryDto
        {
            Name = "Update Category"
        };

        var result = service.Update(category.Id, dto);
        
        Assert.NotNull(result);
        Assert.Equal("Update Category", result.Name);
        
        var savedCategory = context.Categories.Find(result.Id);
        
        Assert.NotNull(savedCategory);
        Assert.Equal("Update Category", savedCategory.Name);
    }

    [Fact]
    public void Update_WhenCategoryDoesNotExist_ReturnsNull()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new CategoryService(context);

        var dto = new UpdateCategoryDto
        {
            Name = "New Category"
        };

        var result = service.Update(999, dto);
        
        Assert.Null(result);
        Assert.Empty(context.Categories);
    }

    [Fact]
    public void ReasignCategory_WhenCategoryExists_ReassignsProductsAndDeletesCategory()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
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

        var product1 = new Product
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = category1.Id
        };

        var product2 = new Product
        {
            Name = "Mouse",
            Stock = 15,
            CategoryId = category1.Id
        };
        
        context.Products.AddRange(product1, product2);
        context.SaveChanges();
        
        var service = new CategoryService(context);
        
        var result = service.ReassignProductsAndDelete(product1.Id, product2.Id);
        
        Assert.Equal(0, result);
        
        Assert.Null(context.Categories.Find(product1.Id));
        Assert.NotNull(context.Categories.Find(product2.Id));
        
        var updatedProduct1 = context.Products.Find(product1.Id);
        var updatedProduct2 = context.Products.Find(product2.Id);
        
        Assert.NotNull(updatedProduct1);
        Assert.NotNull(updatedProduct2);
        
        Assert.Equal(category2.Id, updatedProduct1.CategoryId);
        Assert.Equal(category2.Id, updatedProduct2.CategoryId);
    }

    [Fact]
    public void ReassignProductsAndDelete_WhenSourceAndTargetAreSame_ReturnOne()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);

        var category = new Category
        {
            Name = "Electronics"
        };
        
        context.Categories.Add(category);
        context.SaveChanges();
        
        var service = new CategoryService(context);
        
        var result = service.ReassignProductsAndDelete(category.Id, category.Id);
        
        Assert.Equal(1, result);
        Assert.NotNull(context.Categories.Find(category.Id));
    }

    [Fact]
    public void ReassignProductsAndDelete_WhenSourceCategoryDoesNotExist_ReturnsTwo()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var targetCategory = new Category
        {
            Name = "Electronics"
        };
        
        context.Categories.Add(targetCategory);
        context.SaveChanges();
        
        var service = new CategoryService(context);
        
        var result = service.ReassignProductsAndDelete(999, targetCategory.Id);
        
        Assert.Equal(2, result);
        Assert.NotNull(context.Categories.Find(targetCategory.Id));
        Assert.Single(context.Categories);
    }

    [Fact]
    public void ReassignProductsAndDelete_WenTargetCategoryDoesNotExist_ReturnsThree()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);

        var sourceCategory = new Category
        {
            Name = "Electronics"
        };
        
        context.Categories.Add(sourceCategory);
        context.SaveChanges();

        var product = new Product
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = sourceCategory.Id
        };
        
        context.Products.Add(product);
        context.SaveChanges();
        
        var service = new CategoryService(context);
        
        var result = service.ReassignProductsAndDelete(sourceCategory.Id, 999);
        
        Assert.Equal(3, result);
        
        Assert.NotNull(context.Categories.Find(sourceCategory.Id));
        
        var savedProduct = context.Products.Find(product.Id);
        
        Assert.NotNull(savedProduct);
        Assert.Equal(sourceCategory.Id, savedProduct.CategoryId);
    }

    [Fact]
    public void ReassignProductsAndDelete_PreservesProductsData()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);

        var sourceCategory = new Category
        {
            Name = "Electronics"
        };

        var targetCategory = new Category
        {
            Name = "Accessories"
        };
        
        context.Categories.AddRange(sourceCategory, targetCategory);
        context.SaveChanges();

        var product = new Product
        {
            Name = "Mechanical Keyboard",
            Stock = 25,
            CategoryId = sourceCategory.Id
        };
        
        context.Products.Add(product);
        context.SaveChanges();
        
        var service = new CategoryService(context);
        
        var result = service.ReassignProductsAndDelete(sourceCategory.Id, targetCategory.Id);
        
        var savedProduct = context.Products.Find(product.Id);
        
        Assert.Equal(0, result);
        Assert.NotNull(savedProduct);
        Assert.Equal("Mechanical Keyboard", savedProduct.Name);
        Assert.Equal(25, savedProduct.Stock);
        Assert.Equal(targetCategory.Id, savedProduct.CategoryId);
    }
}