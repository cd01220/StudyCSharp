using System;
using System.Collections.Generic;
using System.Linq;

namespace MyTaLib
{
    // 参考资料： https://school.stockcharts.com/doku.php?id=technical_indicators:parabolic_sar
    public class Sar
    {
        private readonly double optAcc;
        private readonly double optMaxAxx;
        private readonly int maxCyclesNumber;

        private List<Cycle> cycles;
        private double ep;

        public Sar(double optAcc, double optMaxAxx, int maxCyclesNumber)
        {
            this.optAcc = optAcc;
            this.optMaxAxx = optMaxAxx;
            this.maxCyclesNumber = Math.Max(maxCyclesNumber, 2);
        }

        public void SetCycle0(double hi, double lo, double sar, double acc)
        {
            cycles = new List<Cycle>() { new Cycle() { Hi = hi, Lo = lo, Acc = acc, Sar = sar } };
            ep = sar < lo ? hi : lo;
        }

        // return the newest sar value.
        public double AddCycle(double hi, double lo)
        {            
            Cycle prevCycle = cycles.Last();
            if (prevCycle.Sar < prevCycle.Lo)
            {                
                double sar = prevCycle.Sar + (prevCycle.Acc * (ep - prevCycle.Sar));
                if (sar > lo)
                {
                    // 上涨周期 =》 下跌周期
                    sar = Math.Max(Enumerable.Reverse(cycles).Take(maxCyclesNumber - 1).Max(o => o.Hi), hi);
                    cycles.Add(new Cycle() { Hi = hi, Lo = lo, Acc = optAcc, Sar = sar });
                    ep = lo;
                }
                else
                {
                    double acc = Math.Min(prevCycle.Acc + (hi > ep ? optAcc : 0.00d), optMaxAxx);
                    cycles.Add(new Cycle() { Hi = hi, Lo = lo, Acc = acc, Sar = sar });
                    ep = Math.Max(ep, hi);
                }                
            }
            else
            {
                double sar = prevCycle.Sar + (prevCycle.Acc * (ep - prevCycle.Sar));
                if (sar < hi)
                {
                    // 下跌周期 =》 上涨周期
                    sar = Math.Min(Enumerable.Reverse(cycles).Take(maxCyclesNumber - 1).Min(o => o.Lo), lo);
                    cycles.Add(new Cycle() { Hi = hi, Lo = lo, Acc = optAcc, Sar = sar });
                    ep = hi;
                }
                else
                {
                    double acc = Math.Min(prevCycle.Acc + (lo < ep ? optAcc : 0.00d), optMaxAxx);
                    cycles.Add(new Cycle() { Hi = hi, Lo = lo, Acc = acc, Sar = sar });
                    ep = Math.Min(ep, lo);
                }
            }

            return cycles.Last().Sar;
        }

        private class Cycle
        {
            public double Hi;
            public double Lo;

            public double Acc;
            public double Sar;
        }
    }
}
