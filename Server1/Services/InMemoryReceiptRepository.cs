using Server.Services;
using Server.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server1.Services
{
    public class InMemoryReceiptRepository : IReceiptRepository
    {
        private readonly List<Receipt> _store = new();

        public Receipt Add(Receipt receipt)
        {
            _store.Add(receipt);
            return receipt;
        }

        // optional, if you want to list receipts
        public IEnumerable<Receipt> GetAll() => _store;
    }
}