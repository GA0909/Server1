using Server.Models;
using System;
using System.Collections.Generic;

namespace Server.Repositories
{
    public class InMemoryReceiptRepository : IReceiptRepository
    {
        private readonly List<Receipt> _receipts = new();

        public Receipt Create(Receipt receipt)
        {
            _receipts.Add(receipt);
            return receipt;
        }

        public IEnumerable<Receipt> GetAll() => _receipts;

        public Receipt GetById(Guid id) => _receipts.Find(r => r.Id == id);

        public bool Update(Guid id, Receipt receipt)
        {
            var existing = GetById(id);
            if (existing == null) return false;

            _receipts.Remove(existing);
            _receipts.Add(receipt);

            return true;
        }

        public bool Delete(Guid id)
        {
            var receipt = GetById(id);
            return receipt != null && _receipts.Remove(receipt);
        }
    }
}

