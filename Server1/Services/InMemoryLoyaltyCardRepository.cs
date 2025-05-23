using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Services
{
    public class InMemoryLoyaltyCardRepository : ILoyaltyCardRepository
    {
        private readonly List<LoyaltyCard> _loyaltyCards = new List<LoyaltyCard>();

        public IEnumerable<LoyaltyCard> GetAll() => _loyaltyCards;

        public LoyaltyCard GetById(Guid id) => _loyaltyCards.FirstOrDefault(lc => lc.Id == id);

        public LoyaltyCard Create(LoyaltyCard loyaltyCard)
        {
            loyaltyCard.Id = Guid.NewGuid();
            _loyaltyCards.Add(loyaltyCard);
            return loyaltyCard;
        }

        public bool Update(Guid id, LoyaltyCard updatedCard)
        {
            var existing = GetById(id);
            if (existing == null) return false;

            existing.Name = updatedCard.Name;
            existing.Balance = updatedCard.Balance;
            existing.IsActive = updatedCard.IsActive;
            existing.ActivatedAt = updatedCard.ActivatedAt;
            existing.BalanceResetAt = updatedCard.BalanceResetAt;
            return true;
        }

        public bool Delete(Guid id) => _loyaltyCards.RemoveAll(lc => lc.Id == id) > 0;
    }
}
