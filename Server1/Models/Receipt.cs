using System;
using System.Collections.Generic;

namespace Server.Services
{
    public class Receipt
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<ReceiptLine> Lines { get; set; }
        public decimal Total { get; set; }
    }

    public class ReceiptLine
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
    }
}