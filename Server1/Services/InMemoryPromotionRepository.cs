using Server.Models;
using System.Collections.Generic;
using System.Linq;

namespace Server.Services
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
