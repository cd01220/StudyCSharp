using System;
using System.Collections.Generic;
using System.Text;

namespace Changes.ViewModels
{
    public class Ticker
    {
        public virtual decimal AskPrice { get; set; }
        public virtual decimal BidPrice { get; set; }
        public virtual decimal LastPrice { get; set; }
        public virtual DateTime DateTime { get; set; }
    }
}
