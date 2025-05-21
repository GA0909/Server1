using Xunit;
using CashRegister.Core;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace CashRegister.Test
{
    public class ApiProductPromotionTests
    {
        private readonly HttpClient _client;

        public ApiProductPromotionTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new System.Uri("https://localhost:5001/") // Change if needed
            };
        }

        [Fact]
        public async Task Products_WithPromotions_ApplyDiscountsCorrectly()
        {
            // Fetch products
            var productResp = await _client.GetAsync("api/products");
            productResp.EnsureSuccessStatusCode();
            var productJson = await productResp.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<Product>>(productJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Fetch promotions
            var promoResp = await _client.GetAsync("api/promotions");
            promoResp.EnsureSuccessStatusCode();
            var promoJson = await promoResp.Content.ReadAsStringAsync();
            var promotions = JsonSerializer.Deserialize<List<Promotion>>(promoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Simulate selection of 3 random products
            var rand = new System.Random();
            var selected = products.OrderBy(_ => rand.Next()).Take(3).ToList();

            var lines = selected.Select(p => new ReceiptLine
            {
                Product = p,
                Quantity = rand.Next(1, 4),
                UnitPrice = p.Price,
                Discount = 0m
            }).ToList();

            // Apply promotions
            PricingEngine.ApplyPromotions(lines, promotions);

            // Assert that at least one discount was applied if promotions exist
            if (promotions.Any())
            {
                var anyDiscount = lines.Any(l => l.Discount > 0);
                Assert.True(anyDiscount, "At least one product should have a discount applied if promotions exist.");
            }

            // Always assert totals are computable
            var total = ReceiptCalculator.CalculateTotal(lines);
            Assert.True(total >= 0);
        }
    }
}
