using Server.Models;
using System;
using System.Collections.Generic;

namespace Server.Services
{
    public interface IReceiptService
    {
        Receipt CreateReceipt(Receipt receipt);
        IEnumerable<Receipt> GetAllReceipts();
        Receipt GetReceiptById(Guid id);
        bool UpdateReceipt(Guid id, Receipt receipt);
        bool DeleteReceipt(Guid id);
    }
}