using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public class ReceiptDto
    {
        public string ReceiptNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public string DocumentType { get; set; }
        public int KvitasNumber { get; set; }
        public int PosId { get; set; }

        public List<ReceiptLineDto> Items { get; set; } = new List<ReceiptLineDto>();
        public List<PaymentDto> Payments { get; set; } = new List<PaymentDto>();
        public decimal? Change { get; set; }
        public List<VatLineDto> VatLines { get; set; } = new List<VatLineDto>();

        public string RawText { get; set; }  // optional: original block for audit
    }

    public class ReceiptLineDto
    {
        public string Upc { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal? Discount { get; set; }
    }

    public class PaymentDto
    {
        public string Method { get; set; }  // e.g. "Cash", "Card", "SMS", "GiftCard"
        public decimal Amount { get; set; }
    }

    public class VatLineDto
    {
        public int Rate { get; set; }
        public decimal Base { get; set; }
        public decimal Amount { get; set; }  // VAT amount
        public decimal Total { get; set; }  // Base + VAT
    }
}
