using System;

namespace Server.Models
{
    public class GiftCard
    {
        public Guid Id { get; set; }
        public string Name { get; set; } // For alignment with Product
        public string Upc { get; set; } // For alignment with Product
        public double Vat { get; set; } // For alignment with Product
        public decimal Balance { get; set; } //Starting balance == Price when bought as Product
        public bool IsActive { get; set; }
        public DateTime ActivatedAt { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
