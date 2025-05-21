using Xunit;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using CashRegister.Core;

public class ApiIntegrationTests
{
    private readonly HttpClient _client;

    public ApiIntegrationTests()
    {
        _client = new HttpClient
        {
            BaseAddress = new System.Uri("https://localhost:5001/") // Change if your port is different
        };
    }

    [Fact]
    public async Task GetProducts_ReturnsValidList()
    {
        var response = await _client.GetAsync("api/products");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(products);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task GetPromotions_ReturnsValidList()
    {
        var response = await _client.GetAsync("api/promotions");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var promotions = JsonSerializer.Deserialize<List<Promotion>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(promotions);
        // You can remove this if promotions are optional
        // Assert.NotEmpty(promotions); 
    }
}
