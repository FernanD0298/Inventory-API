using System.Net;
using System.Net.Http.Json;
using Inventory_API.DTOs;
using Inventory_API.Models;
using InventoryApi.IntegrationTests.Infrastructure;

namespace InventoryApi.IntegrationTests;

public class CategoriesIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory  _factory;
    private readonly HttpClient _client;
    
    public CategoriesIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task GetCategories_ReturndSuccessStatusCode()
    {
        var response = await _client.GetAsync("/api/categories");
        
        response.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task GetCategories_WhenNoCategoriesExist_ReturnEmptyCollection()
    {
        var response = await _client.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();
        
        var categories = await response.Content.ReadFromJsonAsync<List<Category>>();
        
        Assert.NotNull(categories);
        Assert.Empty(categories);
    }

    [Fact]
    public async Task CreateCategory_ReturnsCreatedCategory()
    {
        var dto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var response = await _client.PostAsJsonAsync("/api/categories", dto);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var category = await response.Content.ReadFromJsonAsync<Category>();
        
        Assert.NotNull(category);
        Assert.True(category.Id > 0);
        Assert.Equal("Electronics", category.Name);
    }

    [Fact]
    public async Task GetCatagoryById_WhenCategoryExists_ReturnsCategory()
    {
        var dto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/categories", dto);
        createResponse.EnsureSuccessStatusCode();
        
        var createdCategory = await createResponse.Content.ReadFromJsonAsync<Category>();
        
        Assert.NotNull(createdCategory);
        
        var response = await _client.GetAsync("/api/categories/" + createdCategory.Id);
        response.EnsureSuccessStatusCode();
        
        var category = await response.Content.ReadFromJsonAsync<Category>();
        
        Assert.NotNull(category);
        Assert.Equal(createdCategory.Id, category.Id);
        Assert.Equal("Electronics", category.Name);
    }

    [Fact]
    public async Task GetCategoryById_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/categories/999");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_WhenCategoryExists_ReturnsUpdatedCategory()
    {
        var dto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/categories", dto);
        createResponse.EnsureSuccessStatusCode();
        
        var createdCategory = await createResponse.Content.ReadFromJsonAsync<Category>();
        
        Assert.NotNull(createdCategory);

        var updateDto = new UpdateCategoryDto
        {
            Name = "Technology"
        };
        
        var updateResponse = await _client.PutAsJsonAsync($"/api/categories/{createdCategory.Id}",  updateDto);
        
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        
        var updateCategory = await updateResponse.Content.ReadFromJsonAsync<Category>();
        
        Assert.NotNull(updateCategory);
        Assert.Equal("Technology", updateCategory.Name);
        Assert.Equal(createdCategory.Id, updateCategory.Id);
    }

    [Fact]
    public async Task UpdateCategory_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var updateDto = new UpdateCategoryDto
        {
            Name = "Technology"
        };
        
        var response = await _client.PutAsJsonAsync("/api/categories/999", updateDto);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteCategory_WhenCategoriesExists_ReturnsNoContent()
    {
        var firstCategory = new Category
        {
            Name = "Electronics"
        };

        var secondCategory = new Category
        {
            Name = "Technology"
        };
        
        var firstResponse = await _client.PostAsJsonAsync("/api/categories", firstCategory);
        var secondResponse = await _client.PostAsJsonAsync("/api/categories", secondCategory);
        
        firstResponse.EnsureSuccessStatusCode();
        secondResponse.EnsureSuccessStatusCode();
        
        var categoryToDelete = await firstResponse.Content.ReadFromJsonAsync<Category>();
        var targetCategory = await secondResponse.Content.ReadFromJsonAsync<Category>();
        
        Assert.NotNull(categoryToDelete);
        Assert.NotNull(targetCategory);

        var dto = new DeleteCategoryDto
        {
            TargetCategoryId = targetCategory.Id
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/categories/" + categoryToDelete.Id)
        {
            Content = JsonContent.Create(dto)
        };
        
        var response = await _client.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        var getResponse = await _client.GetAsync($"/api/categories/{categoryToDelete.Id}");
        
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WhenSourceAndTargetAreSame_ReturnsBadRequest()
    {
        var createDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/categories", createDto);
        createResponse.EnsureSuccessStatusCode();
        
        var category =  await createResponse.Content.ReadFromJsonAsync<Category>();
        
        Assert.NotNull(category);

        var deleteDto = new DeleteCategoryDto
        {
            TargetCategoryId = category.Id
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/categories/" + category.Id)
        {
            Content = JsonContent.Create(createDto)
        };
        
        var response = await _client.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WhenSourceCategoryDoesNotExist_ReturnsNotFound()
    {
        var createDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/categories", createDto);
        createResponse.EnsureSuccessStatusCode();
        
        var targetCategory = await createResponse.Content.ReadFromJsonAsync<Category>();
        
        Assert.NotNull(targetCategory);

        var deleteDto = new DeleteCategoryDto
        {
            TargetCategoryId = targetCategory.Id
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/categories/999")
        {
            Content = JsonContent.Create(deleteDto)
        };
        
        var response = await _client.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WhenTargetCategoryDoesNotExist_ReturnsNotFound()
    {
        var createDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/categories", createDto);
        createResponse.EnsureSuccessStatusCode();
        
        var category = await createResponse.Content.ReadFromJsonAsync<Category>();
        
        Assert.NotNull(category);

        var deleteDto = new DeleteCategoryDto
        {
            TargetCategoryId = 999
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/categories/" + category.Id)
        {
            Content = JsonContent.Create(deleteDto)
        };
        
        var response = await _client.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var getResponse = await _client.GetAsync($"/api/categories/{category.Id}");
        
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}