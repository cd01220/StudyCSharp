using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Changes;
using Changes.ViewModels;
using Microsoft.Reactive.Testing;
using Microsoft.Win32.SafeHandles;
using MyTaLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TicTacTec.TA.Library;
using static TicTacTec.TA.Library.Core;

namespace StudyCSharp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            OkexChanges changes = new OkexChanges();

            int granularity = 3600 * 24;
            List<Candle> candles = changes.GetCandlesAsync("BTC-USD-191227", granularity, null, null, tokenSource.Token).Result;
            Console.WriteLine(JsonConvert.SerializeObject(candles, Formatting.Indented));

            //int count = candles.Count;
            //double[] hi = candles.Select(o => (double)o.HPrice).ToArray();
            //double[] lo = candles.Select(o => (double)o.LPrice).ToArray();
            int count0 = 11;
            double[] hi0 = { 8766.00, 8880.00, 8481.00, 8535.00, 8530.00, 8511.84, 8405.25, 8188.00, 8207.30, 8200.00, 8125.00 };
            double[] lo0 = { 8490.00, 8325.00, 8260.00, 8329.00, 8177.00, 8283.79, 7995.00, 7962.00, 7854.00, 7919.65, 7916.00 };

            Sar sar = new Sar(0.02d, 0.2d, 4);
            sar.SetCycle0(hi0[0], lo0[0], 7751.00d, 0.02d);
            for (int i = 1; i < count0; ++i)
            {
                double value = sar.AddCycle(hi0[i], lo0[i]);
                Console.WriteLine($"hi = {hi0[i]}, low = {lo0[i]}, sar = {value}");
            }

            //Ta-Lib test.
            //int count1 = 11;
            //double[] hi1 = { 8750.00, 8766.00, 8880.00, 8481.00, 8535.00, 8530.00, 8511.84, 8405.25, 8188.00, 8207.30, 8200.00 };
            //double[] lo1 = { 7751.00, 8490.00, 8325.00, 8260.00, 8329.00, 8177.00, 8283.79, 7995.00, 7962.00, 7854.00, 7919.65 };
            //double[] rt1 = new double[count1];

            //int outBegIdx; int outNBElement;
            //double optInAcceleration = 0.02d;
            //double optInMaximum = 0.2d;
            //RetCode code = Core.Sar(0, count1 - 1, hi1, lo1, optInAcceleration, optInMaximum, out outBegIdx, out outNBElement, rt1);
        }
    }
}
