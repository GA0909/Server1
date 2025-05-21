using Server1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server1.Services
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly List<Product> _store = new();

        public IEnumerable<Product> GetAll() => _store;

        public Product GetById(int id) =>
            _store.FirstOrDefault(p => p.Id == id);

        public Product Add(Product p)
        {
            p.Id = _store.Count > 0 ? _store.Max(x => x.Id) + 1 : 1;
            _store.Add(p);
            return p;
        }

        public void Update(int id, Product p)
        {
            var existing = GetById(id);
            if (existing == null) return;
            existing.Name = p.Name;
            existing.Upc = p.Upc;
            existing.Price = p.Price;
            existing.Vat = p.Vat;
        }
    }
}
