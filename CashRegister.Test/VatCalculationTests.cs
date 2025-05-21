using Xunit;
using CashRegister.Core;
using System.Collections.Generic;

namespace CashRegister.Test
{
    public class VatCalculationTests
    {
        [Fact]
        public void SingleItem_21PercentVat_CalculatesCorrectly()
        {
            var line = new ReceiptLine
            {
                Product = new Product { Name = "Item A", Price = 121m, Vat = 21 },
                Quantity = 1,
                UnitPrice = 121m,
                Discount = 0m
            };

            var vatLines = ReceiptCalculator.ComputeVatLines(new List<ReceiptLine> { line });

            Assert.Single(vatLines);
            Assert.Equal(21, vatLines[0].Rate);
            Assert.Equal(100m, vatLines[0].Base);
            Assert.Equal(21m, vatLines[0].Amount);
            Assert.Equal(121m, vatLines[0].Total);
        }

        [Fact]
        public void MultipleVatRates_CalculateSeparately()
        {
            var lines = new List<ReceiptLine>
        {
            new ReceiptLine
            {
                Product = new Product { Name = "Item A", Price = 121m, Vat = 21 },
                Quantity = 1,
                UnitPrice = 121m
            },
            new ReceiptLine
            {
                Product = new Product { Name = "Item B", Price = 112m, Vat = 12 },
                Quantity = 1,
                UnitPrice = 112m
            }
        };

            var vatLines = ReceiptCalculator.ComputeVatLines(lines);

            Assert.Equal(2, vatLines.Count);

            var vat21 = vatLines.Find(v => v.Rate == 21);
            var vat12 = vatLines.Find(v => v.Rate == 12);

            Assert.Equal(100m, vat21.Base);
            Assert.Equal(21m, vat21.Amount);

            Assert.Equal(100m, vat12.Base);
            Assert.Equal(12m, vat12.Amount);
        }

        [Fact]
        public void ZeroVat_ShouldStillProduceEntry()
        {
            var line = new ReceiptLine
            {
                Product = new Product { Name = "Book", Price = 50m, Vat = 0 },
                Quantity = 1,
                UnitPrice = 50m
            };

            var vatLines = ReceiptCalculator.ComputeVatLines(new List<ReceiptLine> { line });

            Assert.Single(vatLines);
            Assert.Equal(0, vatLines[0].Rate);
            Assert.Equal(50m, vatLines[0].Base);
            Assert.Equal(0m, vatLines[0].Amount);
            Assert.Equal(50m, vatLines[0].Total);
        }

        [Fact]
        public void VatCalculation_RoundsCorrectly_OnFractions()
        {
            var line = new ReceiptLine
            {
                Product = new Product { Name = "MicroItem", Price = 1.01m, Vat = 21 },
                Quantity = 1,
                UnitPrice = 1.01m
            };

            var vatLines = ReceiptCalculator.ComputeVatLines(new List<ReceiptLine> { line });

            Assert.Single(vatLines);
            Assert.Equal(21, vatLines[0].Rate);
            Assert.Equal(0.83m, vatLines[0].Base);
            Assert.Equal(0.18m, vatLines[0].Amount);
            Assert.Equal(1.01m, vatLines[0].Total);
        }
    }
}
