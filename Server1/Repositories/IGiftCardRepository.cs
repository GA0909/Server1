using Server.Models;
using System;
using System.Collections.Generic;

namespace Server.Services
{
    public interface IGiftCardRepository
    {
        IEnumerable<GiftCard> GetAll();
        GiftCard GetById(Guid id);
        GiftCard Create(GiftCard giftCard);
        bool Update(Guid id, GiftCard giftCard);
        bool Delete(Guid id);
    }
}
