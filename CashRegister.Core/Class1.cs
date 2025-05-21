using System;
using System.Collections.Generic;
using System.Linq;

namespace CashRegister.Core
{
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
    }

    public class ReceiptLine
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
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

    public class ReceiptDto
    {
        public string ReceiptNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public string DocumentType { get; set; }
        public int KvitasNumber { get; set; }
        public int PosId { get; set; }
        public List<ReceiptLine> Items { get; set; } = new();
        public List<PaymentEntry> Payments { get; set; } = new();
        public decimal? Change { get; set; }
        public List<VatLineDto> VatLines { get; set; } = new();
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

    public static class ReceiptCalculator
    {
        public static decimal CalculateTotal(List<ReceiptLine> lines)
        {
            return lines.Sum(l => l.UnitPrice * l.Quantity - l.Discount);
        }

        public static List<VatLineDto> ComputeVatLines(List<ReceiptLine> lines)
        {
            return lines.GroupBy(l => l.Product.Vat).Select(g =>
            {
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
        }
    }

    public class PaymentProcessor
    {
        public decimal RemainingToPay { get; private set; }
        public List<PaymentEntry> Payments { get; private set; } = new();
        public decimal? Change { get; private set; } = null;

        public PaymentProcessor(decimal initialTotal)
        {
            RemainingToPay = initialTotal;
        }

        public bool TryApplyGiftCard(string cardId, decimal availableBalance, decimal requestedAmount)
        {
            if (requestedAmount > 0 && requestedAmount <= availableBalance && requestedAmount <= RemainingToPay)
            {
                Payments.Add(new PaymentEntry { Method = $"GiftCard-{cardId}", Amount = requestedAmount });
                RemainingToPay -= requestedAmount;
                return true;
            }
            return false;
        }

        public bool TryApplyLoyalty(string loyaltyId, decimal availableBalance, decimal requestedAmount)
        {
            if (requestedAmount > 0 && requestedAmount <= availableBalance && requestedAmount <= RemainingToPay)
            {
                Payments.Add(new PaymentEntry { Method = $"LoyaltyCard-{loyaltyId}", Amount = requestedAmount });
                RemainingToPay -= requestedAmount;
                return true;
            }
            return false;
        }

        public void PayWithCard(decimal amount)
        {
            if (amount > 0 && amount <= RemainingToPay)
            {
                Payments.Add(new PaymentEntry { Method = "Card", Amount = amount });
                RemainingToPay -= amount;
            }
        }

        public void PayWithCash(decimal providedAmount)
        {
            if (providedAmount >= RemainingToPay)
            {
                Payments.Add(new PaymentEntry { Method = "Cash", Amount = providedAmount });
                Change = providedAmount - RemainingToPay;
                RemainingToPay = 0;
            }
        }

        public decimal TotalPaid() => Payments.Sum(p => p.Amount);
    }
}
