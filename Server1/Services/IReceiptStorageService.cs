using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Services
{
    public interface IReceiptStorageService
    {
        void StoreReceipt(ReceiptDto dto);

        /// <summary>
        /// Optionally retrieve all stored ReceiptDto for verification.
        /// </summary>
        IEnumerable<ReceiptDto> GetAllReceipts();
    }
}
