// Program.cs in your CheckParser project
using Server.Models;            // Receipt, ReceiptLine, Payment, VatLine
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CheckParser
{
    class Program
    {
        private const string ServerBaseUrl = "https://localhost:5001/";

        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: CheckParser <path-to-checks.txt>");
                return;
            }

            // 1. Read & parse the checks
            var text = await File.ReadAllTextAsync(args[0]);
            var parser = new Services.CheckParser();
            var checks = parser.ParseAll(text).ToList();

            // 2. Map to server’s Recipets
            var dtos = checks.Select(ch => new Receipt
            {
                ReceiptNumber = ch.ReceiptNumber,
                Timestamp = ch.Time,
                DocumentType = ch.DocumentType,
                KvitasNumber = ch.KvitasNumber,
                PosId = 2,

                Items = ch.Items.Select(i => new ReceiptLine
                {
                    Upc = i.Upc,
                    Name = i.Name,
                    UnitPrice = i.UnitPrice,
                    Quantity = 1,               // if you need grouping by UPC, adjust
                    Discount = i.Discount
                }).ToList(),

                Payments = ch.Payments.Select(p => new Payment
                {
                    Method = p.Method,
                    Amount = p.Amount
                }).ToList(),

                Change = ch.Change,

                VatLines = ch.VatLines.Select(v => new VatLine
                {
                    Rate = v.Rate,
                    Base = v.Base,
                    Amount = v.Amount,
                    Total = v.Total
                }).ToList(),

                RawText = null  // or store the raw block
            }).ToList();

            // 3. POST them to the server
            using var client = new HttpClient { BaseAddress = new Uri(ServerBaseUrl) };
            var jsonOpts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            foreach (var dto in dtos)
            {
                var json = JsonSerializer.Serialize(dto, jsonOpts);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var resp = await client.PostAsync("api/receipt", content);
                    resp.EnsureSuccessStatusCode();
                    Console.WriteLine($"✔ Posted Receipt {dto.ReceiptNumber}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✘ Failed to post {dto.ReceiptNumber}: {ex.Message}");
                }
            }

            Console.WriteLine("All done.");
        }
    }
}

