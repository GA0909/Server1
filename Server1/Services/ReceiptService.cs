using Server.Models;
using Server.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IProductRepository _productRepo;
        private readonly IPromotionRepository _promoRepo;
        private readonly IReceiptRepository _receiptRepo;

        public ReceiptService(
            IProductRepository productRepo,
            IPromotionRepository promoRepo,
            IReceiptRepository receiptRepo)
        {
            _productRepo = productRepo;
            _promoRepo = promoRepo;
            _receiptRepo = receiptRepo;
        }

        public Receipt CreateReceipt(Receipt receipt)
        {
            // Upsert products
            foreach (var line in receipt.Items)
            {
                var existing = _productRepo.GetAll()
                    .FirstOrDefault(p => p.Upc == line.Upc);

                if (existing == null)
                {
                    var newProd = new Product
                    {
                        Name = line.Name,
                        Upc = line.Upc,
                        Price = line.UnitPrice,
                        Vat = receipt.VatLines
                            .FirstOrDefault(v => v.Total == line.Total)?.Rate ?? 21
                    };
                    _productRepo.Add(newProd);
                }
            }

            // Upsert promotions for discounts
            foreach (var line in receipt.Items.Where(i => i.Discount.HasValue && i.Discount != 0))
            {
                var promo = new Promotion
                {
                    Description = $"AutoDiscount on UPC {line.Upc}",
                    Type = "FlatOff",
                    Value = Math.Abs(line.Discount.Value),
                    Start = receipt.Timestamp.AddMinutes(-1),
                    End = receipt.Timestamp.AddMinutes(1)
                };
                _promoRepo.Add(promo);
            }

            // Store Receipt
            receipt.Id = Guid.NewGuid();
            receipt.Timestamp = receipt.Timestamp == default ? DateTime.UtcNow : receipt.Timestamp;

            return _receiptRepo.Create(receipt);
        }

        public IEnumerable<Receipt> GetAllReceipts() => _receiptRepo.GetAll();

        public Receipt GetReceiptById(Guid id) => _receiptRepo.GetById(id);

        public bool UpdateReceipt(Guid id, Receipt receipt)
        {
            return _receiptRepo.Update(id, receipt);
        }

        public bool DeleteReceipt(Guid id)
        {
            return _receiptRepo.Delete(id);
        }
    }
}
