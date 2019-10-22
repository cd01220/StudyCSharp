using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyCSharp.DataModels
{
    public class Candle
    {
        public DateTime DateTime { get; set; }
        public decimal OPrice { get; set; }
        public decimal HPrice { get; set; }
        public decimal LPrice { get; set; }
        public decimal CPrice { get; set; }
        public decimal Volume { get; set; }
    }
}
