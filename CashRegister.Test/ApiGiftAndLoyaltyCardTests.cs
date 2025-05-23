using Server.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace CashRegister.Test
{
    public class ApiGiftAndLoyaltyCardTests
    {
        private readonly HttpClient _client;

        public ApiGiftAndLoyaltyCardTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5001/")
            };
        }

        [Fact]
        public async Task GiftCard_Lifecycle_WorksCorrectly()
        {
            // Create new gift card (purchase simulation)
            var gift = new GiftCard
            {
                Name = "GIFT-TEST-001",
                Upc = "GIFT-TEST-001",
                Balance = 50m,
                IsActive = false,
                ActivatedAt = DateTime.MinValue,
                ValidUntil = DateTime.MinValue.AddYears(1)
            };

            var createResp = await _client.PostAsJsonAsync("api/giftcards", gift);
            createResp.EnsureSuccessStatusCode();

            // Fetch the gift card to get its ID
            var cards = await _client.GetFromJsonAsync<List<GiftCard>>("api/giftcards");
            var stored = cards.Find(c => c.Upc == gift.Upc);
            Assert.NotNull(stored);

            // Activate and set timestamps
            stored.IsActive = true;
            stored.ActivatedAt = DateTime.UtcNow;
            stored.ValidUntil = stored.ActivatedAt.AddYears(1);
            await _client.PutAsJsonAsync($"api/giftcards/{stored.Id}", stored);

            // Simulate usage: deduct from balance
            var usedAmount = 20m;
            stored.Balance -= usedAmount;
            var updateResp = await _client.PutAsJsonAsync($"api/giftcards/{stored.Id}", stored);
            updateResp.EnsureSuccessStatusCode();

            // Final check
            var updated = (await _client.GetFromJsonAsync<List<GiftCard>>("api/giftcards")).Find(c => c.Id == stored.Id);
            Assert.Equal(30m, updated.Balance);
            Assert.True(updated.IsActive);
            Assert.True((DateTime.UtcNow - updated.ActivatedAt).TotalSeconds < 60);
        }

        [Fact]
        public async Task LoyaltyCard_Lifecycle_WorksCorrectly()
        {
            // Create new loyalty card
            var loyalty = new LoyaltyCard
            {
                Name = "LOYAL-TEST-001",
                Balance = 0m,
                IsActive = true,
                ActivatedAt = DateTime.UtcNow,
                BalanceResetAt = DateTime.UtcNow.AddYears(1)
            };

            var createResp = await _client.PostAsJsonAsync("api/loyaltycards", loyalty);
            createResp.EnsureSuccessStatusCode();

            // Add balance (simulate earning points)
            var cards = await _client.GetFromJsonAsync<List<LoyaltyCard>>("api/loyaltycards");
            var stored = cards.Find(c => c.Name == loyalty.Name);
            Assert.NotNull(stored);

            stored.Balance += 15.50m;
            var updateResp = await _client.PutAsJsonAsync($"api/loyaltycards/{stored.Id}", stored);
            updateResp.EnsureSuccessStatusCode();

            // Check updated balance
            var updated = (await _client.GetFromJsonAsync<List<LoyaltyCard>>("api/loyaltycards")).Find(c => c.Id == stored.Id);
            Assert.Equal(15.50m, updated.Balance);
            Assert.True(updated.IsActive);
        }
    }
}
