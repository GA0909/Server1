using Server.Models;
using System.Collections.Generic;

namespace Server.Services
{
    public interface IProductRepository
    {
        IEnumerable<Product> GetAll();
        Product GetById(int id);
        Product Add(Product p);
        void Update(int id, Product p);
    }
}
