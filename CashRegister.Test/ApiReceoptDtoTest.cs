using Xunit;
using CashRegister.Core;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace CashRegister.Test
{
    public class ApiReceiptDtoTests
    {
        private readonly HttpClient _client;

        public ApiReceiptDtoTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5001/")
            };
        }

        [Fact]
        public async Task BuildReceiptDto_FromApiData_ShouldBeValid()
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

            // Select products
            var rand = new Random();
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

            // Compute totals
            var total = ReceiptCalculator.CalculateTotal(lines);
            var vatLines = ReceiptCalculator.ComputeVatLines(lines);

            // Build receipt DTO
            var receipt = new ReceiptDto
            {
                ReceiptNumber = rand.Next(100000, 999999).ToString(),
                Timestamp = DateTime.Now,
                DocumentType = "Receipt",
                KvitasNumber = rand.Next(100000, 999999),
                PosId = 1,
                Items = lines,
                Payments = new List<PaymentEntry> { new PaymentEntry { Method = "Card", Amount = total } },
                Change = 0m,
                VatLines = vatLines
            };

            // Assertions
            Assert.NotNull(receipt);
            Assert.Equal(3, receipt.Items.Count);
            Assert.Single(receipt.Payments);
            Assert.True(receipt.Payments[0].Amount >= 0);
            Assert.True(receipt.VatLines.Count > 0);
            Assert.Equal("Receipt", receipt.DocumentType);

            // Output to console
            Console.WriteLine($"Receipt #{receipt.ReceiptNumber} @ {receipt.Timestamp}");
            foreach (var item in receipt.Items)
            {
                Console.WriteLine($"{item.Quantity} x {item.Product.Name} @ {item.UnitPrice:C} - Discount: {item.Discount:C}");
            }
            Console.WriteLine($"Total Paid: {receipt.Payments[0].Amount:C}, Change: {receipt.Change:C}");
            foreach (var vat in receipt.VatLines)
            {
                Console.WriteLine($"VAT {vat.Rate}% - Base: {vat.Base:C}, Amount: {vat.Amount:C}, Total: {vat.Total:C}");
            }
        }
    }
}
