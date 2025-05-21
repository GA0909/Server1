using System.Collections.Generic;

namespace Server.Services
{
    public interface IReceiptRepository
    {
        Receipt Add(Receipt receipt);
        IEnumerable<Receipt> GetAll();
    }
}