using Server.Models;
using System.Collections.Generic;

namespace Server.Services
{
    public interface IPromotionRepository
    {
        IEnumerable<Promotion> GetAll();
        Promotion Add(Promotion promo);
    }
}
