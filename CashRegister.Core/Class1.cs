using Server.Models; // All shared models (Product, Promotion, Receipt, etc.) live here
using System;
using System.Collections.Generic;
using System.Linq;

namespace CashRegister.Core
{
    public static class PricingEngine
    {
        public static void ApplyPromotions(List<ReceiptLine> lines, List<Promotion> promotions)
        {
            foreach (var line in lines)
            {
                foreach (var promo in promotions)
                {
                    if (promo.Type == "PercentOff")
                        line.Discount += line.UnitPrice * line.Quantity * (promo.Value / 100m);
                    else if (promo.Type == "OneThirdOff")
                        line.Discount += (line.UnitPrice * line.Quantity) / 3m;
                    else if (promo.Type == "FlatOff" &&
                             promo.Description.Contains("on UPC") &&
                             promo.Description.Split("on UPC")[1].Trim() == line.Upc)
                    {
                        line.Discount += promo.Value;
                    }
                }
            }
        }
    }

    public static class ReceiptCalculator
    {
        public static decimal CalculateTotal(List<ReceiptLine> lines)
        {
            return lines.Sum(l => l.UnitPrice * l.Quantity - (l.Discount ?? 0));
        }

        public static List<VatLine> ComputeVatLines(List<ReceiptLine> lines, List<Product> products)
        {
            return lines.GroupBy(line =>
            {
                var matchingProduct = products.FirstOrDefault(p => p.Upc == line.Upc);
                return matchingProduct != null ? (int)matchingProduct.Vat : 0;
            })
            .Select(g =>
            {
                var rate = g.Key;
                var total = g.Sum(l => l.UnitPrice * l.Quantity - (l.Discount ?? 0));
                var baseVal = total / (1m + (decimal)rate / 100m);
                return new VatLine
                {
                    Rate = rate,
                    Base = Math.Round(baseVal, 2),
                    Amount = Math.Round(total - baseVal, 2),
                    Total = Math.Round(total, 2)
                };
            }).ToList();
        }
    }

    public class PaymentProcessor //to show remaining price when using multiple payment methods
    {
        public decimal RemainingToPay { get; private set; }
        public List<Payment> Payments { get; private set; } = new();
        public decimal? Change { get; private set; } = null;

        public PaymentProcessor(decimal initialTotal)
        {
            RemainingToPay = initialTotal;
        }

        public bool TryApplyGiftCard(string cardId, decimal availableBalance, decimal requestedAmount)
        {
            if (requestedAmount > 0 && requestedAmount <= availableBalance && requestedAmount <= RemainingToPay)
            {
                Payments.Add(new Payment { Method = $"GiftCard-{cardId}", Amount = requestedAmount });
                RemainingToPay -= requestedAmount;
                return true;
            }
            return false;
        }

        public bool TryApplyLoyalty(string loyaltyId, decimal availableBalance, decimal requestedAmount)
        {
            if (requestedAmount > 0 && requestedAmount <= availableBalance && requestedAmount <= RemainingToPay)
            {
                Payments.Add(new Payment { Method = $"LoyaltyCard-{loyaltyId}", Amount = requestedAmount });
                RemainingToPay -= requestedAmount;
                return true;
            }
            return false;
        }

        public void PayWithCard(decimal amount)
        {
            if (amount > 0 && amount <= RemainingToPay)
            {
                Payments.Add(new Payment { Method = "Card", Amount = amount });
                RemainingToPay -= amount;
            }
        }

        public void PayWithCash(decimal providedAmount)
        {
            if (providedAmount >= RemainingToPay)
            {
                Payments.Add(new Payment { Method = "Cash", Amount = providedAmount });
                Change = providedAmount - RemainingToPay;
                RemainingToPay = 0;
            }
        }

        public decimal TotalPaid() => Payments.Sum(p => p.Amount);
    }
}