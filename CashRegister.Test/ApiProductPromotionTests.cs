using CashRegister.Core;
using Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace CashRegister.Test
{
    public class ApiProductPromotionTests
    {
        private readonly HttpClient _client;

        public ApiProductPromotionTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new System.Uri("https://localhost:5001/")
            };
        }

        [Fact]
        public async Task Products_WithPromotions_ApplyDiscountsCorrectly()
        {
            var products = await _client.GetFromJsonAsync<List<Product>>("api/products");
            var promotions = await _client.GetFromJsonAsync<List<Promotion>>("api/promotions");

            Assert.NotNull(products);
            Assert.NotNull(promotions);

            var rand = new System.Random();
            var selected = products.OrderBy(_ => rand.Next()).Take(3).ToList();

            var lines = selected.Select(p => new ReceiptLine
            {
                Name = p.Name,
                Upc = p.Upc,
                Quantity = rand.Next(1, 4),
                UnitPrice = p.Price,
                Discount = 0m
            }).ToList();

            PricingEngine.ApplyPromotions(lines, promotions);

            if (promotions.Any())
            {
                var anyDiscount = lines.Any(l => l.Discount > 0);
                Assert.True(anyDiscount, "Expected at least one discount to be applied.");
            }

            var total = ReceiptCalculator.CalculateTotal(lines);
            Assert.True(total >= 0);
        }
    }
}

