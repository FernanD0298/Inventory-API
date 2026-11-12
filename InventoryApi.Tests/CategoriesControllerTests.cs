using Inventory_API.Controllers;
using Inventory_API.Data;
using Inventory_API.DTOs;
using Inventory_API.Models;
using Inventory_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Tests;

public class CategoriesControllerTests
{
    private InventoryDbContext CreateTestContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>().UseSqlite(connection).Options;
        var context = new InventoryDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
    
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

    [Fact]
    public void GetCategories_WhenCategoriesExist_ReturnsOkWhitCategories()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        context.Categories.AddRange(new Category{ Name = "Electronics" },  new Category{ Name = "Accessories" });
        context.SaveChanges();
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);
        
        var result = controller.GetCategories();
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var categories = Assert.IsType<List<Category>>(okResult.Value);
        
        Assert.Equal(2, categories.Count);
    }
    
    [Fact]
    public void GetCategories_WhenNoCategoriesExist_ReturnsOkWithEmptyCollection()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);
        
        var result = controller.GetCategories();
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var categories = Assert.IsType<List<Category>>(okResult.Value);
        
        Assert.Empty(categories);
    }

    [Fact]
    public void GetCategoryById_WhenCategoryExists_ReturnsOkWithCategory()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context, "Electronics");
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);
        
        var result = controller.GetCategoryById(category.Id);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategory = Assert.IsType<CategoryDetailsDto>(okResult.Value);
        
        Assert.Equal(category.Id, returnedCategory.Id);
        Assert.Equal("Electronics", returnedCategory.Name);
    }

    [Fact]
    public void GetCategoryById_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);
        
        var result = controller.GetCategoryById(999);
        
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void CreateCategory_WhenCalled_ReturnsCreatedAction()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);
        
        var dto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var result = controller.CreateCategory(dto);
        
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedCategory = Assert.IsType<Category>(createdResult.Value);
        
        Assert.Equal("GetCategoryById", createdResult.ActionName);
        Assert.Equal("Electronics", returnedCategory.Name);
        Assert.True(returnedCategory.Id > 0);
        
        Assert.Single(context.Categories);
    }
    
    [Fact]
    public void UpdateCategory_WhenCategoryExists_ReturnsOkWithUpdatedCategory()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context, "Electronics");
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);

        var dto = new UpdateCategoryDto
        {
            Name = "Technology"
        };
        
        var result = controller.UpdateCategory(category.Id, dto);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategory = Assert.IsType<Category>(okResult.Value);
        
        Assert.Equal("Technology", returnedCategory.Name);
        Assert.Equal("Technology", context.Categories.Find(category.Id)!.Name);
    }

    [Fact]
    public void UpdateCategory_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);

        var dto = new UpdateCategoryDto
        {
            Name = "Technology"
        };

        var result = controller.UpdateCategory(999, dto);
        
        Assert.IsType<NotFoundResult>(result);
    }
    
    [Fact]
    public void DeleteCategory_WhenCategoryExists_ReturnsNoContent()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var categoryToDelete = CreateTestCategory(context, "Electronics");
        var targetCategory = CreateTestCategory(context, "Technology");
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);

        var dto = new DeleteCategoryDto
        {
            TargetCategoryId = targetCategory.Id
        };
        
        var result = controller.DeleteCategory(categoryToDelete.Id, dto);
        
        Assert.IsType<NoContentResult>(result);
        
        Assert.Null(context.Categories.Find(categoryToDelete.Id));
        Assert.NotNull(context.Categories.Find(targetCategory.Id));
    }

    [Fact]
    public void DeleteCategory_WhenProductsExist_ReassignProductsAndReturnsNoContent()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var categoryToDelete = CreateTestCategory(context, "Electronics");
        var targetCategory = CreateTestCategory(context, "Technology");

        var product = new Product
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = categoryToDelete.Id
        };
        
        context.Products.Add(product);
        context.SaveChanges();
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);

        var dto = new DeleteCategoryDto
        {
            TargetCategoryId = targetCategory.Id
        };
        
        var result = controller.DeleteCategory(product.Id, dto);
        
        Assert.IsType<NoContentResult>(result);
        
        var updatedProduct = context.Products.Find(product.Id);
        
        Assert.NotNull(updatedProduct);
        Assert.Equal(targetCategory.Id, updatedProduct.CategoryId);
        Assert.Equal("Keyboard", updatedProduct.Name);
        Assert.Equal(10, updatedProduct.Stock);
        
        Assert.Null(context.Categories.Find(categoryToDelete.Id));
    }
    
    [Fact]
    public void DeleteCategory_WhenSourceAndTargetAreSame_ReturnsBadRequest()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context, "Electronics");
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);

        var dto = new DeleteCategoryDto
        {
            TargetCategoryId = category.Id
        };
        
        var result = controller.DeleteCategory(category.Id, dto);
        
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        
        Assert.Equal("The target category must be different from the category being deleted.", badRequestResult.Value);
        
        Assert.NotNull(context.Categories.Find(category.Id));
    }

    [Fact]
    public void DeleteCategory_WhenSourceCategoryDoesNotExist_ReturnsNotFound()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context, "Electronics");
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);

        var dto = new DeleteCategoryDto
        {
            TargetCategoryId = category.Id
        };
        
        var result = controller.DeleteCategory(999, dto);
        
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        
        Assert.Equal("The category to delete was not found.",  notFoundResult.Value);
        
        Assert.NotNull(context.Categories.Find(category.Id));
    }

    [Fact]
    public void DeleteCategory_WhenTargetCategoryDoesNotExist_ReturnsNotFound()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context, "Electronics");
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);

        var dto = new DeleteCategoryDto
        {
            TargetCategoryId = 999
        };
        
        var result = controller.DeleteCategory(category.Id, dto);
        
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        
        Assert.Equal("The target category was not found.", notFoundResult.Value);
        
        Assert.NotNull(context.Categories.Find(category.Id));
    }
    
    [Fact]
    public void DeleteCategory_WhenTargetCategoryDoesNotExist_KeepProductsUnChanged()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        
        var context = CreateTestContext(connection);
        
        var category = CreateTestCategory(context, "Electronics");

        var product = new Product
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = category.Id
        };
        
        context.Products.Add(product);
        context.SaveChanges();
        
        var service = new CategoryService(context);
        var controller = new CategoriesController(service);

        var dto = new DeleteCategoryDto
        {
            TargetCategoryId = 999
        };
        
        var result = controller.DeleteCategory(category.Id, dto);
        
        Assert.IsType<NotFoundObjectResult>(result);
        
        var unchangedProduct = context.Products.Find(product.Id);
        
        Assert.NotNull(unchangedProduct);
        Assert.Equal(category.Id, unchangedProduct.CategoryId);
        Assert.Equal(10, unchangedProduct.Stock);
        
        Assert.NotNull(context.Categories.Find(category.Id));
    }
}