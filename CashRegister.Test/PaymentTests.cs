using CashRegister.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CashRegister.Test
{
    public class PaymentTests
    {
        [Fact]
        public void GiftCard_PartialPayment_ShouldDeductCorrectly()
        {
            // Arrange
            var processor = new PaymentProcessor(initialTotal: 50m);

            // Act
            var success = processor.TryApplyGiftCard("GIFT-A", 25m, 20m);

            // Assert
            Assert.True(success);
            Assert.Equal(30m, processor.RemainingToPay);
            Assert.Single(processor.Payments);
            Assert.Equal("GiftCard-GIFT-A", processor.Payments[0].Method);
            Assert.Equal(20m, processor.Payments[0].Amount);
        }

        [Fact]
        public void GiftCard_Overpayment_ShouldBeRejected()
        {
            // Arrange
            var processor = new PaymentProcessor(initialTotal: 50m);

            // Act
            var success = processor.TryApplyGiftCard("GIFT-B", 40m, 60m);

            // Assert
            Assert.False(success);
            Assert.Equal(50m, processor.RemainingToPay);
            Assert.Empty(processor.Payments);
        }

        [Fact]
        public void LoyaltyCard_Payment_And_EarnPoints()
        {
            // Arrange
            var processor = new PaymentProcessor(initialTotal: 100m);
            var available = 15m;

            // Act
            var applied = processor.TryApplyLoyalty("LOYALTY-1", available, 10m);
            processor.PayWithCard(90m);
            decimal earned = System.Math.Floor(processor.TotalPaid()) * 0.01m; // 100 * 0.01

            // Assert
            Assert.True(applied);
            Assert.Equal(0m, processor.RemainingToPay);
            Assert.Equal(2, processor.Payments.Count);
            Assert.Equal(1.00m, earned);
        }

        [Fact]
        public void CardAndCash_SplitCorrectly_AndChangeCalculated()
        {
            // Arrange
            var processor = new PaymentProcessor(initialTotal: 75m);

            // Act
            processor.PayWithCard(30m);
            processor.PayWithCash(50m); // should result in change

            // Assert
            Assert.Equal(0m, processor.RemainingToPay);
            Assert.Equal(2, processor.Payments.Count);
            Assert.Equal("Card", processor.Payments[0].Method);
            Assert.Equal("Cash", processor.Payments[1].Method);
            Assert.Equal(5m, processor.Change); // 30 + 50 - 75 = 5
        }
    }
}
