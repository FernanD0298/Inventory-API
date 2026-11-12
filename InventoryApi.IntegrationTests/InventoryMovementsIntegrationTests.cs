using System.Net;
using System.Net.Http.Json;
using Inventory_API.DTOs;
using Inventory_API.Models;
using InventoryApi.IntegrationTests.Infrastructure;

namespace InventoryApi.IntegrationTests;

public class InventoryMovementsIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    
    public InventoryMovementsIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task GetMovements_ReturnSuccessStatusCode()
    {
        var response = await _client.GetAsync("/api/inventory/movements");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMovements_WhenNoMevementsExist_ReturnEmptyCollection()
    {
        var response = await _client.GetAsync("/api/inventory/movements");
        response.EnsureSuccessStatusCode();

        var movements = await response.Content.ReadFromJsonAsync<List<InventoryMovement>>();
        
        Assert.NotNull(movements);
        Assert.Empty(movements);
    }

    [Fact]
    public async Task AddEntry_WhenProductExists_ReturnSuccess()
    {
        var categoryDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
        var category  = await categoryResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 0,
            CategoryId = category.Id
        };
        
        var productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        var product = await productResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);

        var entryDto = new InventoryEntryDto
        {
            ProductId = product.Id,
            Quantity = 10
        };
        
        var response = await _client.PostAsJsonAsync("/api/inventory/entries", entryDto);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddEntry_IncreasesProductStock()
    {
        var categoryDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
        var category  = await categoryResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 5,
            CategoryId = category.Id
        };
        
        var productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        var product = await productResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);

        var entryDto = new InventoryEntryDto
        {
            ProductId = product.Id,
            Quantity = 10
        };
        
        var response = await _client.PostAsJsonAsync("/api/inventory/entries", entryDto);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var getResponse = await _client.GetAsync("/api/products/" + product.Id);
        
        var updateProduct = await getResponse.Content.ReadFromJsonAsync<Product>();
        
        Assert.NotNull(updateProduct);
        Assert.Equal(15, updateProduct.Stock);
    }

    [Fact]
    public async Task AddEntry_WhenProductDoesNotExists_ReturnNotFound()
    {
        var entryDto = new InventoryEntryDto
        {
            ProductId = 999,
            Quantity = 10
        };
        
        var response = await _client.PostAsJsonAsync("/api/inventory/entries", entryDto);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddExit_WhenStockIsSufficient_ReturnsSuccess()
    {
        var categoryDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
        var category  = await categoryResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 20,
            CategoryId = category.Id
        };
        
        var productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        var product = await productResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);

        var exitDto = new InventoryExitDto
        {
            ProductId = product.Id,
            Quantity = 5
        };
        
        var response = await _client.PostAsJsonAsync("/api/inventory/exits", exitDto);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddExit_DecreasesProductStock()
    {
        var categoryDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
        var category  = await categoryResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 20,
            CategoryId = category.Id
        };
        
        var productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        var product = await productResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);

        var exitDto = new InventoryExitDto
        {
            ProductId = product.Id,
            Quantity = 5
        };
        
        var response = await _client.PostAsJsonAsync("/api/inventory/exits", exitDto);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var getResponse = await _client.GetAsync("/api/products/" + product.Id);
        
        var updateProduct = await getResponse.Content.ReadFromJsonAsync<Product>();
        
        Assert.NotNull(updateProduct);
        Assert.Equal(15, updateProduct.Stock);
    }

    [Fact]
    public async Task AddExit_WhenStockIsInsufficient_ReturnsBadRequest()
    {
        var categoryDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
        var category  = await categoryResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 5,
            CategoryId = category.Id
        };
        
        var productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        var product = await productResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);

        var exitDto = new InventoryExitDto
        {
            ProductId = product.Id,
            Quantity = 10
        };
        
        var response = await _client.PostAsJsonAsync("/api/inventory/exits", exitDto);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var getResponse = await _client.GetAsync("/api/products/" + product.Id);
        
        var unchangeProduct = await getResponse.Content.ReadFromJsonAsync<Product>();
        
        Assert.NotNull(unchangeProduct);
        Assert.Equal(5, unchangeProduct.Stock);
    }

    [Fact]
    public async Task AddExit_WhenProductDoesNotExists_ReturnNotFound()
    {
        var exitDto = new InventoryExitDto
        {
            ProductId = 999,
            Quantity = 5
        };
        
        var response = await _client.PostAsJsonAsync("/api/inventory/exits", exitDto);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMovementsByProduct_WhenProductExists_ReturnsMovements()
    {
        var categoryDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
        var category  = await categoryResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 5,
            CategoryId = category.Id
        };
        
        var productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        var product = await productResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);

        var entryDto = new InventoryEntryDto
        {
            ProductId = product.Id,
            Quantity = 10
        };
        
        await _client.PostAsJsonAsync("/api/inventory/entries", entryDto);
        var response = await _client.GetAsync($"/api/inventory/products/{product.Id}/movements/");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var movements = await response.Content.ReadFromJsonAsync<List<InventoryMovement>>();
        
        Assert.NotNull(movements);
        Assert.NotEmpty(movements);
        Assert.All(movements, movement => Assert.Equal(product.Id, movement.ProductId));
    }

    [Fact]
    public async Task GetMovementsByProduct_WhenProductDoesNotExists_ReturnNotFound()
    {
        var response = await _client.GetAsync($"api/inventory/products/{999}/movements");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMovementsByProduct_WhenProductIdIsInvalid_ReturnsBadRequest()
    {
        var response = await _client.GetAsync($"api/inventory/products/{0}/movements");
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMovements_WhenMovementsExist_ReturnsAllMovements()
    {
        var categoryDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
        var category  = await categoryResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = category.Id
        };
        
        var productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        var product = await productResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);

        var entryDto = new InventoryEntryDto
        {
            ProductId = product.Id,
            Quantity = 5
        };
        
        var exitDto = new InventoryExitDto
        {
            ProductId = product.Id,
            Quantity = 3
        };
        
       await _client.PostAsJsonAsync("/api/inventory/entries", entryDto);
       await _client.PostAsJsonAsync("/api/inventory/exits", exitDto);
       
       var response = await _client.GetAsync($"/api/inventory/movements/");
       var movements = await response.Content.ReadFromJsonAsync<List<InventoryMovement>>();
       
       Assert.NotNull(movements);
       Assert.Equal(2, movements.Count);
       Assert.All(movements, movement => Assert.Equal(product.Id, movement.ProductId));
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}