using Server.Models;
using System;
using System.Collections.Generic;

namespace Server.Repositories
{
    public interface IReceiptRepository
    {
        Receipt Create(Receipt receipt);
        IEnumerable<Receipt> GetAll();
        Receipt GetById(Guid id);
        bool Update(Guid id, Receipt receipt);
        bool Delete(Guid id);
    }
}
