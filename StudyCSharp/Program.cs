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

            //int granularity = 3600 * 24;
            //List<Candle> candles = changes.GetCandlesAsync("BTC-USD-191227", granularity, null, null, tokenSource.Token).Result;
            //Console.WriteLine(JsonConvert.SerializeObject(candles, Formatting.Indented));

            //int count = candles.Count;
            //double[] hi = candles.Select(o => (double)o.HPrice).ToArray();
            //double[] lo = candles.Select(o => (double)o.LPrice).ToArray();
            int count = 11;
            double[] hi = { 8750.00, 8766.00, 8880.00, 8481.00, 8535.00, 8530.00, 8511.84, 8405.25, 8188.00, 8207.30, 8200.00 };
            double[] lo = { 7751.00, 8490.00, 8325.00, 8260.00, 8329.00, 8177.00, 8283.79, 7995.00, 7962.00, 7854.00, 7919.65 };
            double[] rt = new double[count];

            int outBegIdx; int outNBElement;
            double optInAcceleration = 0.02d;
            double optInMaximum = 0.2d;
            RetCode code = Core.Sar(0, count - 1, hi, lo, optInAcceleration, optInMaximum, out outBegIdx, out outNBElement, rt);
        }
    }
}
