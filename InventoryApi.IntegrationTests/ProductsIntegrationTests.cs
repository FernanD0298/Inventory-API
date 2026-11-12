using System.Net;
using System.Net.Http.Json;
using Inventory_API.DTOs;
using Inventory_API.Models;
using InventoryApi.IntegrationTests.Infrastructure;

namespace InventoryApi.IntegrationTests;

public class ProductsIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProductsIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task GetProducts_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/api/products");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WhenNoProductsExist_ReturnsEmptyCollection()
    {
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
        
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        
        Assert.NotNull(products);
        Assert.Empty(products);
    }

    [Fact]
    public async Task CreateProduct_ReturnsCreatedProduct()
    {
        var categoryDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
        categoryResponse.EnsureSuccessStatusCode();
        
        var category = await categoryResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = category.Id
        };
        
        var productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        Assert.Equal(HttpStatusCode.Created, productResponse.StatusCode);
        
        var product = await productResponse.Content.ReadFromJsonAsync<Product>();
        
        Assert.NotNull(product);
        Assert.Equal("Keyboard", product.Name);
        Assert.Equal(10, product.Stock);
        Assert.Equal(category.Id, product.CategoryId);
    }

    [Fact]
    public async Task CreateProduct_WhenCategoryDoesNotExist_ReturnsBadRequest()
    {
        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = 999
        };
        
        var response = await _client.PostAsJsonAsync("/api/products", productDto);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_WhenProductExists_ReturnsProduct()
    {
        var categoryDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
        categoryResponse.EnsureSuccessStatusCode();
        
        var category = await categoryResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);

        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = category.Id
        };
        
        var productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        var createdProduct = await productResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(createdProduct);
        
        var response = await _client.GetAsync($"/api/products/{createdProduct.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var  product = await response.Content.ReadFromJsonAsync<Product>();
        
        Assert.NotNull(product);
        Assert.Equal("Keyboard", product.Name);
        Assert.Equal(10, product.Stock);
        Assert.Equal(category.Id, product.CategoryId);
    }

    [Fact]
    public async Task GetProductById_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/products/999");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WhenProductExists_ReturnsUpdatedProduct()
    {
        var categoryDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
        categoryResponse.EnsureSuccessStatusCode();
        
        var category = await categoryResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);
        
        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = category.Id
        };
        
        var productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        var createdProduct = await productResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(createdProduct);

        var updateDto = new UpdateProductDto
        {
            Name = "Mechanical Keyboard",
            Stock = 20,
            CategoryId = category.Id
        };
        
        var response = await _client.PutAsJsonAsync($"/api/products/{createdProduct.Id}", updateDto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var updatedProduct = await response.Content.ReadFromJsonAsync<Product>();
        
        Assert.NotNull(updatedProduct);
        Assert.Equal("Mechanical Keyboard", updatedProduct.Name);
        Assert.Equal(20, updatedProduct.Stock);
    }

    [Fact]
    public async Task UpdateProducts_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var updateDto = new UpdateProductDto
        {
            Name = "Mechanical Keyboard",
            Stock = 20,
            CategoryId = 1
        }; 
        
        var response = await _client.PutAsJsonAsync("/api/products/999", updateDto);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WhenCategoryDoesNotExist_ReturnsBadRequest()
    {
        var categoryDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
        var category = await categoryResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);
        
        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = category.Id
        };
        
        var productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        var createdProduct = await productResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(createdProduct);
        
        var updateDto = new UpdateProductDto
        {
            Name = "Mechanical Keyboard",
            Stock = 20,
            CategoryId = 999
        }; 
        
        var response = await _client.PutAsJsonAsync($"/api/products/{createdProduct.Id}", updateDto);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WhenProductExists_ReturnsNoContent()
    {
        var categoryDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };
        
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", categoryDto);
        var category = await categoryResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);
        
        var productDto = new CreateProductDto
        {
            Name = "Keyboard",
            Stock = 10,
            CategoryId = category.Id
        };
        
        var productResponse = await _client.PostAsJsonAsync("/api/products", productDto);
        var createdProduct = await productResponse.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(createdProduct);
        
        var response = await _client.DeleteAsync($"/api/products/{createdProduct.Id}");
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        var getResponse = await _client.GetAsync($"/api/products/{createdProduct.Id}");
        
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/products/999");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}