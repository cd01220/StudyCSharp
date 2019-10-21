using System;
using System.Collections.Generic;
using System.Text;

namespace MyTaLib
{
    // Example:
    //CancellationTokenSource tokenSource = new CancellationTokenSource();
    //OkexChanges changes = new OkexChanges();

    //IEnumerable<DataModels.Candle> candles = changes.GetCandlesAsync("BTC-USD-191227", 3600 * 24, null, null, tokenSource.Token)
    //    .Result
    //    .SkipWhile(o => o.DateTime <TimeZone.CurrentTimeZone.ToUniversalTime(new DateTime(2018, 12, 28)))
    //    .Select(o => o.Adapt<DataModels.Candle>())
    //    .ToList();

    //DataModels.Candle prevCandle = candles.First();
    //prevCandle.Ema5 = 3694.40d;
    //prevCandle.Ema7 = 3689.17d;
    //prevCandle.Ema10 = 3672.91d;
    //foreach (DataModels.Candle candle in candles.Skip(1))
    //{
    //    candle.Ema5 = MyTaLib.Ema.CalculateEma(5, prevCandle.Ema5, (double)candle.CPrice);
    //    candle.Ema7 = MyTaLib.Ema.CalculateEma(7, prevCandle.Ema7, (double)candle.CPrice);
    //    candle.Ema10 = MyTaLib.Ema.CalculateEma(10, prevCandle.Ema10, (double)candle.CPrice);
    //    Console.WriteLine($"CPrice = {candle.CPrice}, ema5 = {candle.Ema5}, ema7 = {candle.Ema7}, ema10 = {candle.Ema10}");
    //    prevCandle = candle;
    //}
    public class Ema
    {
        public static double CalculateEma(int n, double prevCycleEma, double closePrice)
        {
            double a = 2.0d / (n + 1.0d);
            return (a * closePrice) + ((1.0d - a) * prevCycleEma);
        }

        public static double CalculatePriceWhenEmaXtoY(int n1, int n2, double n1Ema, double n2Ema)
        {
            double a1 = 2.0d / (n1 + 1.0d);
            double a2 = 2.0d / (n2 + 1.0d);
            return (((1 - a2) * n2Ema) - ((1 - a1) * n1Ema)) / (a1 - a2);
        }
    }
}
