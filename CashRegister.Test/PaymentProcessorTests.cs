using CashRegister.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            new ReceiptLine { Product = products[0], Quantity = 1, UnitPrice = products[0].Price },
            new ReceiptLine { Product = products[1], Quantity = 2, UnitPrice = products[1].Price }
        };

            // Act
            PricingEngine.ApplyPromotions(lines, promotions);
            var total = ReceiptCalculator.CalculateTotal(lines);

            // Assert
            // Expected: 10 - 5 (discount) + 20*2 = 45.00
            Assert.Equal(45.00m, total);
        }
    }
}
