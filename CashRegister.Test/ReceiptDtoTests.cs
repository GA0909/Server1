using CashRegister.Core;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CashRegister.Test
{
    public class ReceiptDtoTests
    {
        [Fact]
        public void ReceiptDto_BuildsCorrectly_WithBasicData()
        {
            // Arrange
            var products = new List<Product>
        {
            new Product { Name = "Item A", Upc = "A1", Price = 20m, Vat = 21 },
            new Product { Name = "Item B", Upc = "B2", Price = 30m, Vat = 21 }
        };

            var lines = new List<ReceiptLine>
        {
            new ReceiptLine { Name = products[0].Name, Upc = products[0].Upc, Quantity = 2, UnitPrice = 20m, Discount = 5m },
            new ReceiptLine { Name = products[1].Name, Upc = products[0].Upc, Quantity = 1, UnitPrice = 30m, Discount = 0m }
        };

            var payments = new List<Payment>
        {
            new Payment { Method = "Card", Amount = 65m }
        };

            var vatLines = ReceiptCalculator.ComputeVatLines(lines, products);

            // Act
            var receipt = new Receipt
            {
                ReceiptNumber = "123456",
                Timestamp = DateTime.Now,
                DocumentType = "Receipt",
                KvitasNumber = 654321,
                PosId = 1,
                Items = lines,
                Payments = payments,
                Change = 0m,
                VatLines = vatLines
            };

            // Assert
            Assert.Equal("123456", receipt.ReceiptNumber);
            Assert.Equal(2, receipt.Items.Count);
            Assert.Single(receipt.Payments);
            Assert.Equal("Card", receipt.Payments[0].Method);
            Assert.Equal(65m, receipt.Payments[0].Amount);
            Assert.Single(receipt.VatLines);
            Assert.Equal(21, receipt.VatLines[0].Rate);
        }

        [Fact]
        public void ReceiptDto_HandlesChangeCalculation()
        {
            var products = new List<Product>
        {
            new Product { Name = "Soda", Upc = "A1", Price = 1.50m, Vat = 0 },
        };
            // Arrange
            var lines = new List<ReceiptLine>
            {
                new ReceiptLine { Name = products[0].Name, Quantity = 2, UnitPrice = products[0].Price}
            };

            var payments = new List<Payment>
        {
            new Payment { Method = "Cash", Amount = 10.00m }
        };

            var subTotal = ReceiptCalculator.CalculateTotal(lines);
            var change = payments.Sum(p => p.Amount) - subTotal;

            // Act
            var receipt = new Receipt
            {
                ReceiptNumber = "987654",
                Timestamp = DateTime.Now,
                DocumentType = "Receipt",
                KvitasNumber = 111111,
                PosId = 5,
                Items = lines,
                Payments = payments,
                Change = Math.Round(change, 2),
                VatLines = ReceiptCalculator.ComputeVatLines(lines, products)
            };

            // Assert
            Assert.Equal("987654", receipt.ReceiptNumber);
            Assert.Equal(7.00m, receipt.Change);
            Assert.Equal(3.00m, ReceiptCalculator.CalculateTotal(lines));
        }
    }
}
