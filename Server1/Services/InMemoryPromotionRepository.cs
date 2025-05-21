using Server1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server1.Services
{
    public class InMemoryPromotionRepository : IPromotionRepository
    {
        private readonly List<Promotion> _store = new();

        public IEnumerable<Promotion> GetAll() => _store;

        public Promotion Add(Promotion promo)
        {
            // assign a simple incremental Id
            promo.Id = _store.Count > 0 ? _store.Max(x => x.Id) + 1 : 1;
            _store.Add(promo);
            return promo;
        }
    }
}
