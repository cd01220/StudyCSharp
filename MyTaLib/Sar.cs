using System;
using System.Collections.Generic;
using System.Linq;

namespace MyTaLib
{
    // 参考资料： https://school.stockcharts.com/doku.php?id=technical_indicators:parabolic_sar
    // Example:
    //CancellationTokenSource tokenSource = new CancellationTokenSource();
    //OkexChanges changes = new OkexChanges();

    //IEnumerable<Candle> candles = changes.GetCandlesAsync("BTC-USD-191227", 3600 * 24, null, null, tokenSource.Token)
    //    .Result
    //    .SkipWhile(o => o.DateTime < TimeZone.CurrentTimeZone.ToUniversalTime(new DateTime(2018, 12, 28)));
    //Console.WriteLine(JsonConvert.SerializeObject(candles, Formatting.Indented));

    //Sar sar = new Sar(0.02d, 0.2d, 4);
    //sar.SetCycle0((double)candles.First().HPrice, (double)candles.First().LPrice, 3974.99d, 0.02d);
    //foreach (Candle candle in candles.Skip(1))
    //{
    //    double value = sar.AddCycle((double)candle.HPrice, (double)candle.LPrice);
    //    Console.WriteLine($"hi = {candle.HPrice}, low = {candle.LPrice}, sar = {value}");
    //}
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

            double sar;
            if (prevCycle.Sar < prevCycle.Lo)
            {                
                sar = prevCycle.Sar + (prevCycle.Acc * (ep - prevCycle.Sar));
                if (sar > lo)
                {
                    // 上涨周期 =》 下跌周期
                    sar = Math.Max(Enumerable.Reverse(cycles).Take(maxCyclesNumber - 1).Max(o => o.Hi), hi);
                    cycles.Clear();
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
                sar = prevCycle.Sar + (prevCycle.Acc * (ep - prevCycle.Sar));
                if (sar < hi)
                {
                    // 下跌周期 =》 上涨周期
                    sar = Math.Min(Enumerable.Reverse(cycles).Take(maxCyclesNumber - 1).Min(o => o.Lo), lo);
                    cycles.Clear();
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

            return sar;
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
