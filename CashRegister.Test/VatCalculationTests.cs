using CashRegister.Core;
using Server.Models;
using System.Collections.Generic;
using Xunit;

namespace CashRegister.Test
{
    public class VatCalculationTests
    {
        [Fact]
        public void SingleItem_21PercentVat_CalculatesCorrectly()
        {
            var product = new Product { Name = "Item A", Upc = "001", Price = 121m, Vat = 21 };
            var line = new ReceiptLine
            {
                Name = product.Name,
                Upc = product.Upc,
                Quantity = 1,
                UnitPrice = product.Price,
                Discount = 0m
            };

            var lines = new List<ReceiptLine> { line };
            var products = new List<Product> { product };

            var vatLines = ReceiptCalculator.ComputeVatLines(lines, products);

            Assert.Single(vatLines);
            Assert.Equal(21, vatLines[0].Rate);
            Assert.Equal(100m, vatLines[0].Base);
            Assert.Equal(21m, vatLines[0].Amount);
            Assert.Equal(121m, vatLines[0].Total);
        }

        [Fact]
        public void MultipleVatRates_CalculateSeparately()
        {
            var producta = new Product { Name = "Item A", Upc = "001", Price = 121m, Vat = 21 };
            var productb = new Product { Name = "Item B", Upc = "002", Price = 112m, Vat = 12 };
            var linea = new ReceiptLine
            {
                Name = producta.Name,
                Upc = producta.Upc,
                Quantity = 1,
                UnitPrice = producta.Price,
                Discount = 0m
            };
            var lineb = new ReceiptLine
            {
                Name = productb.Name,
                Upc = productb.Upc,
                Quantity = 1,
                UnitPrice = productb.Price,
                Discount = 0m
            };
            var lines = new List<ReceiptLine> { linea, lineb };
            var products = new List<Product> { producta, productb };

            var vatLines = ReceiptCalculator.ComputeVatLines(lines, products);

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
            var product = new Product { Name = "Item A", Upc = "001", Price = 50m, Vat = 0 };
            var line = new ReceiptLine
            {
                Name = product.Name,
                Upc = product.Upc,
                Quantity = 1,
                UnitPrice = product.Price,
                Discount = 0m
            };

            var lines = new List<ReceiptLine> { line };
            var products = new List<Product> { product };

            var vatLines = ReceiptCalculator.ComputeVatLines(lines, products);

            Assert.Single(vatLines);
            Assert.Equal(0, vatLines[0].Rate);
            Assert.Equal(50m, vatLines[0].Base);
            Assert.Equal(0m, vatLines[0].Amount);
            Assert.Equal(50m, vatLines[0].Total);
        }

        [Fact]
        public void VatCalculation_RoundsCorrectly_OnFractions()
        {

            var product = new Product { Name = "Item A", Upc = "001", Price = 1.01m, Vat = 21 };
            var line = new ReceiptLine
            {
                Name = product.Name,
                Upc = product.Upc,
                Quantity = 1,
                UnitPrice = product.Price,
                Discount = 0m
            };

            var lines = new List<ReceiptLine> { line };
            var products = new List<Product> { product };

            var vatLines = ReceiptCalculator.ComputeVatLines(lines, products);


            Assert.Single(vatLines);
            Assert.Equal(21, vatLines[0].Rate);
            Assert.Equal(0.83m, vatLines[0].Base);
            Assert.Equal(0.18m, vatLines[0].Amount);
            Assert.Equal(1.01m, vatLines[0].Total);
        }
    }
}
