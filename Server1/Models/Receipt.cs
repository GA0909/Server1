using System;
using System.Collections.Generic;

namespace Server.Models
{
    public class Receipt
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ReceiptNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public string DocumentType { get; set; }
        public int KvitasNumber { get; set; }
        public int PosId { get; set; }

        public List<ReceiptLine> Items { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
        public decimal? Change { get; set; }
        public List<VatLine> VatLines { get; set; } = new();

        public string RawText { get; set; }  // Optional audit/logging text
    }

    public class ReceiptLine
    {
        public string Upc { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal? Discount { get; set; }

        // Optional convenience property
        public decimal Total => (UnitPrice * Quantity) - (Discount ?? 0);
    }

    public class Payment
    {
        public string Method { get; set; }  // e.g., "Cash", "Card", "SMS", "GiftCard"
        public decimal Amount { get; set; }
    }

    public class VatLine
    {
        public int Rate { get; set; }           // VAT rate (e.g., 21%)
        public decimal Base { get; set; }       // Amount before VAT
        public decimal Amount { get; set; }     // VAT amount
        public decimal Total { get; set; }      // Base + VAT
    }
}
