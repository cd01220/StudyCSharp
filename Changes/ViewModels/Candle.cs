using System;
using System.Collections.Generic;
using System.Text;

namespace Changes.ViewModels
{
    public class Candle
    {
        public virtual DateTime DateTime { get; set; }
        public virtual decimal OPrice { get; set; }
        public virtual decimal CPrice { get; set; }
        public virtual decimal HPrice { get; set; }
        public virtual decimal LPrice { get; set; }
        public virtual decimal Volume { get; set; }
    }
}
