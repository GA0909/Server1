// Project: CashRegisterRandom (Console App)
// File: Program.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CashRegisterRandom
{
    class Program
    {
        private static readonly HttpClient _client = new HttpClient { BaseAddress = new Uri("https://localhost:5001/") };

        static async Task Main(string[] args)
        {
            // Fetch products and promotions
            var products = await FetchAsync<List<Product>>("api/products");
            var promotions = await FetchAsync<List<Promotion>>("api/promotions");

            // Gift cards - simulation of active and inactive cards
            var giftCards = new Dictionary<string, (bool Active, decimal Balance)>
            {
                { "GIFT10", (false, 10m) },
                { "GIFT20", (false, 20m) },
                { "GIFT50", (false, 50m) },
                { "GIFT100", (false, 100m) },
                { "GIFT-ACTIVE-1", (true, 15.69m) },
                { "GIFT-ACTIVE-2", (true, 25.40m) }
            };

            // Loyalty cards
            var loyaltyCards = new Dictionary<string, decimal>
            {
                { "LOYALTY-000", 0.00m },
                { "LOYALTY-015", 0.15m },
                { "LOYALTY-2515", 25.15m }
            };

            var rand = new Random();
            int distinctCount = rand.Next(1, Math.Min(6, products.Count));
            var chosen = products.OrderBy(_ => rand.Next()).Take(distinctCount).ToList();

            var lines = chosen.Select(p => new ReceiptLine
            {
                Product = p,
                Quantity = rand.Next(1, 5),
                UnitPrice = p.Price,
                Discount = 0m
            }).ToList();

            PricingEngine.ApplyPromotions(lines, promotions);

            decimal subTotal = lines.Sum(l => l.UnitPrice * l.Quantity - l.Discount);
            var payments = new List<PaymentEntry>();
            decimal? change = null;
            Console.WriteLine($"Initial total to pay: {subTotal:C}");

            // Scan for gift cards
            Console.WriteLine("Scan Gift Cards (enter code or 'done'):");
            while (true)
            {
                var input = Console.ReadLine();
                if (input.ToLower() == "done") break;
                if (giftCards.TryGetValue(input, out var giftInfo))
                {
                    if (giftInfo.Active)
                    {
                        Console.WriteLine($"Gift Card {input} - Balance: {giftInfo.Balance:C}. Use it? (enter amount or 'no'):");
                        var use = Console.ReadLine();
                        if (decimal.TryParse(use, out decimal amount) && amount > 0 && amount <= giftInfo.Balance && amount <= subTotal)
                        {
                            payments.Add(new PaymentEntry { Method = $"GiftCard-{input}", Amount = amount });
                            subTotal -= amount;
                            Console.WriteLine($"Remaining to pay: {subTotal:C}");
                        }
                        else if (use.ToLower() != "no")
                        {
                            Console.WriteLine("Invalid amount. Skipping this gift card.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Inactive Gift Card Scanned: {input}, added to product list.");
                        var giftProduct = new Product { Name = $"Gift Card {giftInfo.Balance:C}", Upc = input, Price = giftInfo.Balance, Vat = 21 };
                        lines.Add(new ReceiptLine { Product = giftProduct, Quantity = 1, UnitPrice = giftProduct.Price, Discount = 0 });
                        subTotal += giftProduct.Price;
                        Console.WriteLine($"New total to pay after adding product: {subTotal:C}");
                    }
                }
                else
                {
                    Console.WriteLine("Unknown gift card.");
                }
            }

            // Loyalty card
            Console.WriteLine("Scan Loyalty Card (enter code or press Enter to skip):");
            var loyaltyCode = Console.ReadLine();
            decimal loyaltyBalance = 0;
            if (!string.IsNullOrWhiteSpace(loyaltyCode) && loyaltyCards.TryGetValue(loyaltyCode, out loyaltyBalance))
            {
                Console.WriteLine($"Loyalty card scanned. Balance: {loyaltyBalance:C}. Use it? (enter amount or 'no'):");
                var use = Console.ReadLine();
                if (decimal.TryParse(use, out decimal amount) && amount > 0 && amount <= loyaltyBalance && amount <= subTotal)
                {
                    payments.Add(new PaymentEntry { Method = $"LoyaltyCard-{loyaltyCode}", Amount = amount });
                    loyaltyCards[loyaltyCode] -= amount;
                    subTotal -= amount;
                    Console.WriteLine($"Remaining to pay: {subTotal:C}");
                }
                else if (use.ToLower() != "no")
                {
                    Console.WriteLine("Invalid amount. Skipping loyalty card payment.");
                }
                lines.Add(new ReceiptLine
                {
                    Product = new Product { Name = $"Loyalty card scanned. Balance: {loyaltyBalance:C}", Upc = loyaltyCode, Price = 0, Vat = 0 },
                    Quantity = 1,
                    UnitPrice = 0,
                    Discount = 0
                });
            }

            // Final payment with cash/card
            if (subTotal > 0)
            {
                Console.WriteLine($"Remaining to pay: {subTotal:C}. Pay with (cash/card/both):");
                var method = Console.ReadLine().ToLower();

                if (method == "both")
                {
                    Console.Write("Enter amount for Card: ");
                    var cardPart = decimal.Parse(Console.ReadLine());
                    if (cardPart >= 0.01m && cardPart <= subTotal)
                    {
                        payments.Add(new PaymentEntry { Method = "Card", Amount = cardPart });
                        subTotal -= cardPart;
                        Console.WriteLine($"Remaining to pay in cash: {subTotal:C}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid card amount. Paying full amount in cash.");
                    }
                    decimal provided = Math.Round(subTotal + (decimal)rand.NextDouble() * 10m, 2);
                    change = Math.Round(provided - subTotal, 2);
                    payments.Add(new PaymentEntry { Method = "Cash", Amount = provided });
                }
                else if (method == "card")
                {
                    payments.Add(new PaymentEntry { Method = "Card", Amount = subTotal });
                }
                else
                {
                    decimal provided = Math.Round(subTotal + (decimal)rand.NextDouble() * 10m, 2);
                    change = Math.Round(provided - subTotal, 2);
                    payments.Add(new PaymentEntry { Method = "Cash", Amount = provided });
                }
            }

            // VAT
            var vatGroups = lines.GroupBy(l => l.Product.Vat);
            var vatLines = vatGroups.Select(g => {
                var rate = g.Key;
                var total = g.Sum(l => l.UnitPrice * l.Quantity - l.Discount);
                var baseV = total / (1m + (decimal)rate / 100m);
                var vatAmt = total - baseV;
                return new VatLineDto
                {
                    Rate = (int)rate,
                    Base = Math.Round(baseV, 2),
                    Amount = Math.Round(vatAmt, 2),
                    Total = Math.Round(total, 2)
                };
            }).ToList();

            var receiptDto = new ReceiptDto
            {
                ReceiptNumber = rand.Next(100000, 999999).ToString(),
                Timestamp = DateTime.Now,
                DocumentType = "Receipt",
                KvitasNumber = rand.Next(100000, 999999),
                PosId = 1,
                Items = lines.Select(l => new ReceiptLineDto
                {
                    Name = l.Product.Name,
                    Upc = l.Product.Upc,
                    UnitPrice = l.UnitPrice,
                    Quantity = l.Quantity,
                    Discount = l.Discount
                }).ToList(),
                Payments = payments,
                Change = change,
                VatLines = vatLines
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            Console.WriteLine(JsonSerializer.Serialize(receiptDto, options));

            // Loyalty accumulation after payment
            if (!string.IsNullOrWhiteSpace(loyaltyCode) && loyaltyCards.ContainsKey(loyaltyCode))
            {
                decimal paidAmount = payments.Sum(p => p.Amount);
                decimal earned = Math.Floor(paidAmount) * 0.01m;
                loyaltyCards[loyaltyCode] += earned;
                Console.WriteLine($"Loyalty points earned: {earned:C}. New balance: {loyaltyCards[loyaltyCode]:C}");
            }
        }

        private static async Task<T> FetchAsync<T>(string url)
        {
            var resp = await _client.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Upc { get; set; }
        public decimal Price { get; set; }
        public double Vat { get; set; }
    }

    public class Promotion
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class ReceiptLine
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
    }

    public static class PricingEngine
    {
        public static void ApplyPromotions(List<ReceiptLine> lines, List<Promotion> promotions)
        {
            foreach (var line in lines)
            {
                foreach (var promo in promotions)
                {
                    if (promo.Type == "PercentOff")
                    {
                        line.Discount += line.UnitPrice * line.Quantity * (promo.Value / 100m);
                    }
                    else if (promo.Type == "OneThirdOff")
                    {
                        line.Discount += (line.UnitPrice * line.Quantity) / 3m;
                    }
                    else if (promo.Type == "FlatOff")
                    {
                        // Match by UPC if specified in description
                        if (promo.Description.Contains("on UPC") &&
                            promo.Description.Split("on UPC")[1].Trim() == line.Product.Upc)
                        {
                            line.Discount += promo.Value;
                        }
                    }
                }
            }
        }
    }

    public class ReceiptDto
    {
        public string ReceiptNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public string DocumentType { get; set; }
        public int KvitasNumber { get; set; }
        public int PosId { get; set; }
        public List<ReceiptLineDto> Items { get; set; } = new();
        public List<PaymentEntry> Payments { get; set; } = new();
        public decimal? Change { get; set; }
        public List<VatLineDto> VatLines { get; set; } = new();
    }

    public class ReceiptLineDto
    {
        public string Name { get; set; }
        public string Upc { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal? Discount { get; set; }
    }

    public class PaymentEntry
    {
        public string Method { get; set; }
        public decimal Amount { get; set; }
    }

    public class VatLineDto
    {
        public int Rate { get; set; }
        public decimal Base { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
    }
}
