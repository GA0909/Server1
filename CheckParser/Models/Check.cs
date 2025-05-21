using System;
using System.Collections.Generic;

namespace CheckParser.Models
{
    public class Check
    {
        // header
        public string ReceiptNumber { get; set; }
        public DateTime Time { get; set; }
        public string DocumentType { get; set; }
        public int KvitasNumber { get; set; }

        // items
        public List<LineItem> Items { get; set; } = new List<LineItem>();

        // payments (cash, card, gift card, SMS)
        public List<PaymentEntry> Payments { get; set; } = new List<PaymentEntry>();
        public decimal? Change { get; set; } // only for cash

        // VAT
        public List<VatLine> VatLines { get; set; } = new List<VatLine>();

        // footer
        public string DocumentCheckNum { get; set; }
        public string ModuleId { get; set; }
        public string Signature { get; set; }
        public string Code { get; set; }
        public string FiscalReceipt { get; set; }
        public DateTime FiscalTime { get; set; }
    }

    public class LineItem
    {
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? Discount { get; set; }
        public string Upc { get; set; }
    }

    public class VatLine
    {
        public int Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal Base { get; set; }
        public decimal Total { get; set; }
    }

    public class PaymentEntry
    {
        public string Method { get; set; }
        public decimal Amount { get; set; }
    }
}
