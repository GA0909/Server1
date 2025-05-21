using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server1.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // e.g. "PercentOff", "ThreeForTwo"
        public decimal Value { get; set; } // e.g. 10 for 10%, or 0 for 3-for-2
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
