// Program.cs in your CheckParser project
using Server.Models;            // Receipt, ReceiptLine, Payment, VatLine
using System;
using System.Collections.Generic;
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
            else if (args.Length > 0 && args[0].Equals("REPLICATECHECKS", StringComparison.OrdinalIgnoreCase))
            {
                await ReplicateChecks();
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


            //-------------------------------------recreate checks

            static async Task ReplicateChecks()
            {
                using var client = new HttpClient { BaseAddress = new Uri(ServerBaseUrl) };
                var jsonOpts = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                try
                {
                    var response = await client.GetAsync("api/receipt");
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var receipts = JsonSerializer.Deserialize<List<Receipt>>(json, jsonOpts);

                    using var writer = new StreamWriter("built check replicas.txt");
                    int checkCounter = 1;

                    foreach (var receipt in receipts)
                    {
                        await writer.WriteLineAsync($"--------------------CHECK {checkCounter++}-------------------");
                        await writer.WriteLineAsync("UAB \"LINDEX\"");
                        await writer.WriteLineAsync("                Įm. k. 300636460                ");
                        await writer.WriteLineAsync("             PVM k. LT100002964411              ");
                        await writer.WriteLineAsync("               Ozo g. 25, Vilnius               ");
                        await writer.WriteLineAsync("               Tel. nr. 070088091               ");
                        await writer.WriteLineAsync("\tSERVICE_INFO_START");
                        await writer.WriteLineAsync($"\tReceipt: {receipt.ReceiptNumber}");
                        await writer.WriteLineAsync($"\tTime: {receipt.Timestamp:dd.MM.yyyy HH:mm:ss}");
                        await writer.WriteLineAsync($"\tDocument: {receipt.DocumentType}");
                        await writer.WriteLineAsync("\tSERVICE_INFO_END");
                        await writer.WriteLineAsync();
                        await writer.WriteLineAsync($"POS: {receipt.PosId}         CR-000045260        {receipt.KvitasNumber}");
                        await writer.WriteLineAsync($"Kvitas: {receipt.KvitasNumber}");
                        await writer.WriteLineAsync("------------------------------------------------");

                        foreach (var item in receipt.Items)
                        {
                            await writer.WriteLineAsync($"{item.Name.PadRight(40)}{item.UnitPrice,7:N2} A");
                            if (item.Discount > 0)
                            {
                                await writer.WriteLineAsync($"Nuolaida prekei {-item.Discount,27:N2} A");
                            }
                            await writer.WriteLineAsync($"{item.Upc.PadLeft(47)}#");
                            await writer.WriteLineAsync("                                               #");
                        }

                        await writer.WriteLineAsync("------------------------------------------------");
                        var subtotal = receipt.Items.Sum(i => i.UnitPrice * i.Quantity - i.Discount);
                        await writer.WriteLineAsync($"Tarpinė suma EUR{subtotal,28:N2}");

                        foreach (var payment in receipt.Payments)
                        {
                            await writer.WriteLineAsync($"Pateikta {payment.Method}{payment.Amount,30:N2}");
                        }

                        await writer.WriteLineAsync();
                        await writer.WriteLineAsync("---------------------------- PVM  Be PVM  Su PVM");

                        foreach (var vatLine in receipt.VatLines)
                        {
                            await writer.WriteLineAsync($"PVM-{vatLine.Rate,2}%{vatLine.Amount,25:N2}{vatLine.Base,8:N2}{vatLine.Total,8:N2}");
                        }

                        await writer.WriteLineAsync();
                        await writer.WriteLineAsync("----------- INFO KVITO PATIKRINIMUI ------------");
                        await writer.WriteLineAsync($"Dokumento numeris:{receipt.KvitasNumber,27}");
                        await writer.WriteLineAsync("Saugos modulio numeris:                     ");
                        await writer.WriteLineAsync("Kvito parašas:                             ");
                        await writer.WriteLineAsync("Kvito kodas:                               ");
                        await writer.WriteLineAsync();
                        await writer.WriteLineAsync("64132                   12                     #");
                        await writer.WriteLineAsync("572                   1674                     #");
                        await writer.WriteLineAsync();
                        await writer.WriteLineAsync("Saugos Modulio ID:                  00956D18B869");
                        await writer.WriteLineAsync();
                    }

                    Console.WriteLine("All receipts processed and saved.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching receipts: {ex.Message}");
                }
            }
        }
    }
}

