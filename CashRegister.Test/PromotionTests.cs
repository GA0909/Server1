using Xunit;
using System.Collections.Generic;
using CashRegister.Core;

namespace CashRegister.Test
{
    public class PromotionTests
    {
        [Fact]
        public void FlatOff_AppliesCorrectly_WhenUpcMatches()
        {
            var product = new Product { Name = "Shirt", Upc = "123", Price = 50m, Vat = 21 };
            var line = new ReceiptLine { Product = product, Quantity = 1, UnitPrice = product.Price };
            var promo = new Promotion { Type = "FlatOff", Value = 10m, Description = "AutoDiscount on UPC 123" };

            PricingEngine.ApplyPromotions(new List<ReceiptLine> { line }, new List<Promotion> { promo });

            Assert.Equal(10m, line.Discount);
        }

        [Fact]
        public void PercentOff_AppliesCorrectly_ForMultipleQuantity()
        {
            var product = new Product { Name = "Hat", Upc = "456", Price = 20m, Vat = 21 };
            var line = new ReceiptLine { Product = product, Quantity = 3, UnitPrice = product.Price };
            var promo = new Promotion { Type = "PercentOff", Value = 25m, Description = "Summer sale" };

            PricingEngine.ApplyPromotions(new List<ReceiptLine> { line }, new List<Promotion> { promo });

            Assert.Equal(15m, line.Discount); // 20 * 3 * 0.25 = 15
        }

        [Fact]
        public void OneThirdOff_AppliesCorrectly()
        {
            var product = new Product { Name = "Socks", Upc = "789", Price = 9m, Vat = 21 };
            var line = new ReceiptLine { Product = product, Quantity = 2, UnitPrice = product.Price };
            var promo = new Promotion { Type = "OneThirdOff", Value = 0m, Description = "Promo" };

            PricingEngine.ApplyPromotions(new List<ReceiptLine> { line }, new List<Promotion> { promo });

            Assert.Equal(6m, line.Discount); // (9*2)/3
        }

        [Fact]
        public void InvalidUpcInFlatOff_DoesNotApply()
        {
            var product = new Product { Name = "Bag", Upc = "111", Price = 100m, Vat = 21 };
            var line = new ReceiptLine { Product = product, Quantity = 1, UnitPrice = product.Price };
            var promo = new Promotion { Type = "FlatOff", Value = 30m, Description = "AutoDiscount on UPC 999" };

            PricingEngine.ApplyPromotions(new List<ReceiptLine> { line }, new List<Promotion> { promo });

            Assert.Equal(0m, line.Discount);
        }

        [Fact]
        public void StackedPromotions_ApplyAllDiscounts()
        {
            var product = new Product { Name = "Shoes", Upc = "321", Price = 60m, Vat = 21 };
            var line = new ReceiptLine { Product = product, Quantity = 1, UnitPrice = product.Price };

            var promos = new List<Promotion>
        {
            new Promotion { Type = "PercentOff", Value = 10m, Description = "Flash Sale" },  // 60 * 0.10 = 6
            new Promotion { Type = "FlatOff", Value = 5m, Description = "AutoDiscount on UPC 321" }, // 5
            new Promotion { Type = "OneThirdOff", Value = 0, Description = "One third" } // 60 / 3 = 20
        };

            PricingEngine.ApplyPromotions(new List<ReceiptLine> { line }, promos);

            Assert.Equal(31m, line.Discount); // 6 + 5 + 20 = 31
        }
    }
}
