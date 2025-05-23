using CashRegister.Core;
using Server.Models; // Use shared models from Server
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CashRegisterRandom
{
    class Program
    {
        private static readonly HttpClient _client = new() { BaseAddress = new Uri("https://localhost:5001/") };

        static async Task Main(string[] args)
        {
            await InitializeGiftAndLoyaltyCards();

            var rand = new Random();

            var products = await FetchAsync<List<Product>>("api/products");
            var promotions = await FetchAsync<List<Promotion>>("api/promotions");
            var giftCards = await FetchAsync<List<GiftCard>>("api/giftcards");
            var loyaltyCards = await FetchAsync<List<LoyaltyCard>>("api/loyaltycards");

            //Getting randome products
            var lines = products.OrderBy(_ => rand.Next()).Take(rand.Next(1, 6)).Select(p => new ReceiptLine
            {
                Upc = p.Upc,
                Name = p.Name,
                UnitPrice = p.Price,
                Quantity = rand.Next(1, 5),
                Discount = 0m
            }).ToList();

            PricingEngine.ApplyPromotions(lines, promotions);

            decimal subTotal = lines.Sum(l => l.UnitPrice * l.Quantity - (l.Discount ?? 0));
            var payments = new List<Payment>();
            decimal? change = null;

            Console.WriteLine("Scan Gift Cards (enter code or 'done'):");
            while (true)
            {
                var input = Console.ReadLine();
                if (input.ToLower() == "done") break;
                var card = giftCards.FirstOrDefault(c => c.Upc == input);

                if (card != null)
                {
                    if (card.IsActive)
                    {
                        Console.WriteLine($"Gift Card {input} - Balance: {card.Balance:C}. Use it? (enter amount or 'no'):");
                        var use = Console.ReadLine();
                        if (decimal.TryParse(use, out decimal amount) && amount > 0 && amount <= card.Balance && amount <= subTotal)
                        {
                            payments.Add(new Payment { Method = $"GiftCard-{input}", Amount = amount });
                            subTotal -= amount;
                            card.Balance -= amount;
                            await _client.PutAsJsonAsync($"api/giftcards/{card.Id}", card);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Inactive Gift Card. Adding to purchase.");
                        lines.Add(new ReceiptLine { Upc = card.Upc, Name = $"GiftCard {card.Balance:C}", Quantity = 1, UnitPrice = card.Balance, Discount = 0 });
                        subTotal += card.Balance;
                    }
                }
                else Console.WriteLine("Gift card not found.");
            }

            Console.WriteLine("Scan Loyalty Card (enter code or blank to skip):");
            var loyaltyInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(loyaltyInput))
            {
                var loyalty = loyaltyCards.FirstOrDefault(l => l.Name == loyaltyInput);
                if (loyalty != null)
                {
                    Console.WriteLine($"Loyalty Card - Balance: {loyalty.Balance:C}. Use it? (enter amount or 'no'):");
                    var use = Console.ReadLine();
                    if (decimal.TryParse(use, out decimal amount) && amount > 0 && amount <= loyalty.Balance && amount <= subTotal)
                    {
                        payments.Add(new Payment { Method = $"LoyaltyCard-{loyalty.Name}", Amount = amount });
                        subTotal -= amount;
                        loyalty.Balance -= amount;
                        await _client.PutAsJsonAsync($"api/loyaltycards/{loyalty.Id}", loyalty);
                    }
                }
            }

            if (subTotal > 0)
            {
                Console.WriteLine($"Remaining to pay: {subTotal:C}. Pay with (cash/card):");
                var method = Console.ReadLine().ToLower();

                if (method == "card")
                {
                    payments.Add(new Payment { Method = "Card", Amount = subTotal });
                }
                else
                {
                    decimal provided = Math.Round(subTotal + (decimal)rand.NextDouble() * 10m, 2);
                    change = provided - subTotal;
                    payments.Add(new Payment { Method = "Cash", Amount = provided });
                }
            }

            var vatLines = ReceiptCalculator.ComputeVatLines(lines, products);

            var receipt = new Receipt
            {
                ReceiptNumber = rand.Next(100000, 999999).ToString(),
                Timestamp = DateTime.Now,
                DocumentType = "Receipt",
                KvitasNumber = rand.Next(100000, 999999),
                PosId = 1,
                Items = lines,
                Payments = payments,
                Change = change,
                VatLines = vatLines
            };

            var result = await _client.PostAsJsonAsync("api/receipts", receipt);
            Console.WriteLine(result.IsSuccessStatusCode ? "✅ Receipt stored successfully." : "❌ Failed to store receipt.");
        }

        private static async Task<T> FetchAsync<T>(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
        private static async Task InitializeGiftAndLoyaltyCards() //adding giftcards and loyalti cards
        {
            var predefinedGifts = new[] { 10, 20, 50, 100 };
            foreach (var amount in predefinedGifts)
            {
                var giftCard = new GiftCard
                {
                    Name = $"GIFT-{amount}",
                    Upc = $"GIFT-{amount}",
                    Balance = amount,
                    IsActive = false,
                    ActivatedAt = DateTime.MinValue,
                    ValidUntil = DateTime.MinValue.AddYears(1)
                };
                await _client.PostAsJsonAsync("api/giftcards", giftCard);
            }

            var activeGifts = new[] { ("GIFT-ACTIVE-1", 15.69m), ("GIFT-ACTIVE-2", 25.40m) };
            foreach (var (name, balance) in activeGifts)
            {
                var giftCard = new GiftCard
                {
                    Name = name,
                    Upc = name,
                    Balance = balance,
                    IsActive = true,
                    ActivatedAt = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddYears(1)
                };
                await _client.PostAsJsonAsync("api/giftcards", giftCard);
            }

            var loyaltyCards = new[] { ("LOYALTY-000", 0.00m), ("LOYALTY-015", 0.15m), ("LOYALTY-2515", 25.15m) };
            foreach (var (name, balance) in loyaltyCards)
            {
                var loyalty = new LoyaltyCard
                {
                    Name = name,
                    Balance = balance,
                    IsActive = true,
                    ActivatedAt = DateTime.UtcNow,
                    BalanceResetAt = DateTime.UtcNow.AddYears(1)
                };
                await _client.PostAsJsonAsync("api/loyaltycards", loyalty);
            }
        }
    }
}
