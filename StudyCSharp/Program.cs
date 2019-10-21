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
using Mapster;
using Microsoft.Reactive.Testing;
using Microsoft.Win32.SafeHandles;
using MyTaLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TicTacTec.TA.Library;
using static TicTacTec.TA.Library.Core;

namespace StudyCSharp
{
    public class EmaTrading
    {
        public EmaTrading()
        {
        }

        public void Analyze()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            OkexChanges changes = new OkexChanges();

            // 测试BTC季度合约，2小时线，EMA7 EMA10 交叉时买入卖出。
            DateTime dateTime = DateTime.Parse("2019-09-26T20:00:00Z").ToUniversalTime();
            IEnumerable<DataModels.Candle> btcCandles = changes.GetCandlesAsync("BTC-USD-191227", 3600 * 2, null, null, tokenSource.Token)
                .Result
                .SkipWhile(o => o.DateTime < dateTime)
                .Select(o => o.Adapt<DataModels.Candle>())
                .ToList();

            DataModels.Candle prevCandle = btcCandles.First();
            prevCandle.Ema5 = 8194.94d;
            prevCandle.Ema7 = 8227.34d;
            prevCandle.Ema10 = 8276.51d;
            foreach (DataModels.Candle candle in btcCandles.Skip(1))
            {
                candle.Ema5 = MyTaLib.Ema.CalculateEma(5, prevCandle.Ema5, (double)candle.CPrice);
                candle.Ema7 = MyTaLib.Ema.CalculateEma(7, prevCandle.Ema7, (double)candle.CPrice);
                candle.Ema10 = MyTaLib.Ema.CalculateEma(10, prevCandle.Ema10, (double)candle.CPrice);
                prevCandle = candle;
            }
            Console.WriteLine(JsonConvert.SerializeObject(btcCandles, Formatting.Indented));

            DataModels.Candle open = null, close = null;
            prevCandle = btcCandles.First();            
            State state = prevCandle.Ema7 > prevCandle.Ema10 ? State.WaitingForShortOpen : State.WaitingForShortOpen;
            foreach (DataModels.Candle candle in btcCandles.Skip(1))
            {
                double priceWhenXtoY = MyTaLib.Ema.CalculatePriceWhenEmaXtoY(5, 7, prevCandle.Ema7, prevCandle.Ema10);
                switch (state)
                {
                case State.WaitingForLongOpen:
                    if (candle.Ema7 > candle.Ema10)
                    {
                        // 开仓
                        open = candle;
                        state = State.WaitingForLongClose;
                    }
                    break;

                case State.WaitingForLongClose:
                    if (priceWhenXtoY < (double)candle.LPrice)
                    {
                        close = candle;
                        Console.WriteLine($"开多 {open.DateTime}, 开仓价:{open.CPrice}, 平多{close.DateTime}, 平仓价{priceWhenXtoY}");
                        state = State.WaitingForLongOpen;
                    }
                    break;
                }

                prevCandle = candle;
            }
        }

        private enum State
        {
            WaitingForLongOpen, 
            WaitingForShortOpen,
            WaitingForLongClose,
            WaitingForShortClose
        }
    }

    public class Program
    {
        private static void Main(string[] args)
        {
            EmaTrading emaTrading = new EmaTrading();
            emaTrading.Analyze();
        }
    }
}
