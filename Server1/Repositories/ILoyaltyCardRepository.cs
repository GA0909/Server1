using Server.Models;
using System;
using System.Collections.Generic;

namespace Server.Services
{
    public interface ILoyaltyCardRepository
    {
        IEnumerable<LoyaltyCard> GetAll();
        LoyaltyCard GetById(Guid id);
        LoyaltyCard Create(LoyaltyCard loyaltyCard);
        bool Update(Guid id, LoyaltyCard loyaltyCard);
        bool Delete(Guid id);
    }
}
