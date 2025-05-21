using Server.Models;
using Server1.Models;
using Server1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Services
{
    public class ReceiptStorageService : IReceiptStorageService
    {
        private readonly IProductRepository _productRepo;
        private readonly IPromotionRepository _promoRepo;
        private readonly IReceiptRepository _receiptRepo;
        private readonly List<ReceiptDto> _inMemoryDtos = new List<ReceiptDto>();

        public ReceiptStorageService(
            IProductRepository productRepo,
            IPromotionRepository promoRepo,
            IReceiptRepository receiptRepo)
        {
            _productRepo = productRepo;
            _promoRepo = promoRepo;
            _receiptRepo = receiptRepo;
        }

        public void StoreReceipt(ReceiptDto dto)
        {
            // Keep original dto for verification
            _inMemoryDtos.Add(dto);

            // 1. Upsert products
            foreach (var line in dto.Items)
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
                        Vat = dto.VatLines.FirstOrDefault(v => v.Total == (line.UnitPrice * line.Quantity - (line.Discount ?? 0)))?.Rate ?? 21
                    };
                    existing = _productRepo.Add(newProd);
                }
            }

            // 2. Upsert promotions for any discounts
            foreach (var line in dto.Items)
            {
                if (line.Discount.HasValue && line.Discount.Value != 0)
                {
                    var desc = $"AutoDiscount on UPC {line.Upc}";
                    var value = Math.Abs(line.Discount.Value);
                    // create one-day promotion to simulate discount
                    var promo = new Promotion
                    {
                        Description = desc,
                        Type = "FlatOff",
                        Value = value,
                        Start = dto.Timestamp.AddMinutes(-1),
                        End = dto.Timestamp.AddMinutes(1)
                    };
                    _promoRepo.Add(promo);
                }
            }

            // 3. Store receipt lines
            var receipt = new Receipt
            {
                Timestamp = dto.Timestamp,
                Lines = dto.Items.Select(i => new ReceiptLine
                {
                    ProductId = _productRepo.GetAll().First(p => p.Upc == i.Upc).Id,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Discount = i.Discount ?? 0m
                }).ToList(),
                Total = dto.Items.Sum(i => i.UnitPrice * i.Quantity + (i.Discount ?? 0m))
            };
            _receiptRepo.Add(receipt);
        }

        public IEnumerable<ReceiptDto> GetAllReceipts() => _inMemoryDtos;
    }
}
