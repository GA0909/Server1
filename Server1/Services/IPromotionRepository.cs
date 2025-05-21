using Server1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server1.Services
{
    public interface IPromotionRepository
    {
        IEnumerable<Promotion> GetAll();
        Promotion Add(Promotion promo);
    }
}
