using System;

namespace Server.Models
{
    public class LoyaltyCard
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public DateTime ActivatedAt { get; set; }
        public DateTime BalanceResetAt { get; set; }
    }
}
