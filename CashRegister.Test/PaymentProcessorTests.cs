using CashRegister.Core;
using Server.Models;
using System.Collections.Generic;
using Xunit;

namespace CashRegister.Test
{
    public class PaymentProcessorTests
    {
        [Fact]
        public void FetchProductsAndPromotions_CalculatesInitialTotalCorrectly()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Name = "Item A", Upc = "111", Price = 10.00m, Vat = 21 },
                new Product { Name = "Item B", Upc = "222", Price = 20.00m, Vat = 21 }
            };

            var promotions = new List<Promotion>
            {
                new Promotion { Type = "FlatOff", Value = 5.00m, Description = "AutoDiscount on UPC 111" }
            };

            var lines = new List<ReceiptLine>
            {
                new ReceiptLine { Name = products[0].Name, Upc = products[0].Upc, Quantity = 1, UnitPrice = products[0].Price, Discount = 0m },
                new ReceiptLine { Name = products[1].Name, Upc = products[1].Upc, Quantity = 2, UnitPrice = products[1].Price, Discount = 0m }
            };

            // Act
            PricingEngine.ApplyPromotions(lines, promotions);
            var total = ReceiptCalculator.CalculateTotal(lines);

            // Assert
            // Expected: (10 - 5) + (20*2) = 45.00
            Assert.Equal(45.00m, total);
        }
    }
}

