using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Upc { get; set; }    // or SKU
        public decimal Price { get; set; }
        public double Vat { get; set; }    // e.g. 21 for 21%
    }
}
