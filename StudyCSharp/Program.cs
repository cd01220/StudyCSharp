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

            IEnumerable<Candle> candles = changes.GetCandlesAsync("BTC-USD-191227", 3600 * 24, null, null, tokenSource.Token)
                .Result
                .SkipWhile(o => o.DateTime < TimeZone.CurrentTimeZone.ToUniversalTime(new DateTime(2018, 12, 28)));            
            Console.WriteLine(JsonConvert.SerializeObject(candles, Formatting.Indented));

            Sar sar = new Sar(0.02d, 0.2d, 4);
            sar.SetCycle0((double)candles.First().HPrice, (double)candles.First().LPrice, 3974.99d, 0.02d);
            foreach (Candle candle in candles.Skip(1))
            {
                double value = sar.AddCycle((double)candle.HPrice, (double)candle.LPrice);
                Console.WriteLine($"hi = {candle.HPrice}, low = {candle.LPrice}, sar = {value}");
            }
        }
    }
}
