using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Services
{
    public class InMemoryGiftCardRepository : IGiftCardRepository
    {
        private readonly List<GiftCard> _giftCards = new List<GiftCard>();

        public IEnumerable<GiftCard> GetAll() => _giftCards;

        public GiftCard GetById(Guid id) => _giftCards.FirstOrDefault(gc => gc.Id == id);

        public GiftCard Create(GiftCard giftCard)
        {
            giftCard.Id = Guid.NewGuid();
            _giftCards.Add(giftCard);
            return giftCard;
        }

        public bool Update(Guid id, GiftCard updatedGiftCard)
        {
            var existing = GetById(id);
            if (existing == null) return false;

            existing.Name = updatedGiftCard.Name;
            existing.Upc = updatedGiftCard.Upc;
            existing.Vat = updatedGiftCard.Vat;
            existing.Balance = updatedGiftCard.Balance;
            existing.IsActive = updatedGiftCard.IsActive;
            existing.ActivatedAt = updatedGiftCard.ActivatedAt;
            existing.ValidUntil = updatedGiftCard.ValidUntil;
            return true;
        }

        public bool Delete(Guid id) => _giftCards.RemoveAll(gc => gc.Id == id) > 0;
    }
}
