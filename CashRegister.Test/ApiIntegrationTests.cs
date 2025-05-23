using Server.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace CashRegister.Test
{
    public class ApiIntegrationTests
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new System.Uri("https://localhost:5001/") // Change if needed
            };
        }

        [Fact]
        public async Task GetProducts_ReturnsValidList()
        {
            var products = await _client.GetFromJsonAsync<List<Product>>("api/products");
            Assert.NotNull(products);
            Assert.NotEmpty(products);
        }

        [Fact]
        public async Task GetPromotions_ReturnsValidList()
        {
            var promotions = await _client.GetFromJsonAsync<List<Promotion>>("api/promotions");
            Assert.NotNull(promotions);
        }

        [Fact]
        public async Task GetGiftCards_ReturnsList()
        {
            var giftCards = await _client.GetFromJsonAsync<List<GiftCard>>("api/giftcards");
            Assert.NotNull(giftCards);
        }

        [Fact]
        public async Task GetLoyaltyCards_ReturnsList()
        {
            var loyaltyCards = await _client.GetFromJsonAsync<List<LoyaltyCard>>("api/loyaltycards");
            Assert.NotNull(loyaltyCards);
        }
    }
}

