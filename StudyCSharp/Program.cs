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

        private IEnumerable<DataModels.Candle> GetCandles(string Id, DateTime beginTime, DateTime endTime)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            OkexChanges changes = new OkexChanges();

            List<DataModels.Candle> candles = new List<DataModels.Candle>();
            while (beginTime < endTime)
            {
                IEnumerable<DataModels.Candle> cs = changes.GetCandlesAsync(Id, 3600 * 2, beginTime, endTime, tokenSource.Token)
                .Result
                .Select(o => o.Adapt<DataModels.Candle>())
                .ToList();

                candles.AddRange(cs);
                beginTime = beginTime.AddHours(cs.Count() * 2);
            }            

            return candles;
        }

        public void Analyze()
        {
            DateTime beginTime = DateTime.Parse("2019-08-01T00:00:00Z").ToUniversalTime();
            DateTime endTime   = DateTime.Parse("2019-10-22T00:00:00Z").ToUniversalTime();
            // 测试BTC季度合约，2小时线，EMA7 EMA10 交叉时买入卖出。 Get
            IEnumerable<DataModels.Candle> btcCandles = GetCandles("BTC-USD-191227", beginTime, endTime);

            DataModels.Candle prevCandle = btcCandles.First();
            prevCandle.Ema5 = 10037.64d;
            prevCandle.Ema7 = 10010.98d;
            prevCandle.Ema10 = 9964.20d;
            foreach (DataModels.Candle candle in btcCandles.Skip(1))
            {
                candle.Ema5 = MyTaLib.Ema.CalculateEma(5, prevCandle.Ema5, (double)candle.CPrice);
                candle.Ema7 = MyTaLib.Ema.CalculateEma(7, prevCandle.Ema7, (double)candle.CPrice);
                candle.Ema10 = MyTaLib.Ema.CalculateEma(10, prevCandle.Ema10, (double)candle.CPrice);
                prevCandle = candle;
            }
            //Console.WriteLine(JsonConvert.SerializeObject(btcCandles, Formatting.Indented));
            Console.WriteLine("=============BTC0=============");
            AnalyzeImple0(btcCandles);
            Console.WriteLine("=============BTC1=============");
            AnalyzeImple1(btcCandles);
            Console.WriteLine("=============BTC2=============");
            AnalyzeImple2(btcCandles);

            // 测试ETH季度合约，2小时线，EMA7 EMA10 交叉时买入卖出。
            IEnumerable<DataModels.Candle> ethCandles = GetCandles("ETH-USD-191227", beginTime, endTime);
            prevCandle = ethCandles.First();
            prevCandle.Ema5 = 215.844d;
            prevCandle.Ema7 = 215.656d;
            prevCandle.Ema10 = 215.193d;
            foreach (DataModels.Candle candle in ethCandles.Skip(1))
            {
                candle.Ema5 = MyTaLib.Ema.CalculateEma(5, prevCandle.Ema5, (double)candle.CPrice);
                candle.Ema7 = MyTaLib.Ema.CalculateEma(7, prevCandle.Ema7, (double)candle.CPrice);
                candle.Ema10 = MyTaLib.Ema.CalculateEma(10, prevCandle.Ema10, (double)candle.CPrice);
                prevCandle = candle;
            }
            //Console.WriteLine(JsonConvert.SerializeObject(ethCandles, Formatting.Indented));
            Console.WriteLine("=============ETH0=============");
            AnalyzeImple0(ethCandles);
            Console.WriteLine("=============ETH1=============");
            AnalyzeImple1(ethCandles);
            Console.WriteLine("=============ETH2=============");
            AnalyzeImple2(ethCandles);

            // 测试EOS季度合约，2小时线，EMA7 EMA10 交叉时买入卖出。            
            IEnumerable<DataModels.Candle> eosCandles = GetCandles("EOS-USD-191227", beginTime, endTime);
            prevCandle = eosCandles.First();
            prevCandle.Ema5 = 4.358d;
            prevCandle.Ema7 = 4.344d;
            prevCandle.Ema10 = 4.325d;
            foreach (DataModels.Candle candle in eosCandles.Skip(1))
            {
                candle.Ema5 = MyTaLib.Ema.CalculateEma(5, prevCandle.Ema5, (double)candle.CPrice);
                candle.Ema7 = MyTaLib.Ema.CalculateEma(7, prevCandle.Ema7, (double)candle.CPrice);
                candle.Ema10 = MyTaLib.Ema.CalculateEma(10, prevCandle.Ema10, (double)candle.CPrice);
                prevCandle = candle;
            }
            //Console.WriteLine(JsonConvert.SerializeObject(eosCandles, Formatting.Indented));
            Console.WriteLine("=============EOS0=============");
            AnalyzeImple0(eosCandles);
            Console.WriteLine("=============EOS1=============");
            AnalyzeImple1(eosCandles);
            Console.WriteLine("=============EOS2=============");
            AnalyzeImple2(eosCandles);
        }

        private void AnalyzeImple0(IEnumerable<DataModels.Candle> candles)
        {
            double totleProfile = 0.0d;
            DataModels.Candle open = null, close = null;
            DataModels.Candle prevCandle = candles.First();
            State state = prevCandle.Ema7 > prevCandle.Ema10 ? State.WaitingForShortOpen : State.WaitingForLongOpen;
            foreach (DataModels.Candle candle in candles.Skip(1))
            {
                double priceWhenXtoY = MyTaLib.Ema.CalculatePriceWhenEmaXtoY(7, 10, prevCandle.Ema7, prevCandle.Ema10);
                switch (state)
                {
                case State.WaitingForLongOpen:
                    if (candle.Ema7 > candle.Ema10)
                    {
                        // 开多
                        open = candle;
                        state = State.WaitingForLongClose;
                    }
                    break;

                case State.WaitingForLongClose:
                    if ((double)candle.LPrice < priceWhenXtoY)
                    {
                        close = candle;
                        double profile = (priceWhenXtoY - (double)open.CPrice) / (double)open.CPrice * 100;
                        totleProfile = totleProfile + profile;
                        Console.WriteLine($"开多:{open.DateTime:s}, 开仓价:{open.CPrice:f2}, 平多:{close.DateTime:s}, 平仓价{priceWhenXtoY:f2}, 盈亏{profile:f2}%");
                        if (candle.Ema7 < candle.Ema10)
                        {
                            open = candle;
                            state = State.WaitingForShortClose;
                        }
                        else
                        {
                            state = State.WaitingForShortOpen;
                        }
                    }
                    break;

                case State.WaitingForShortOpen:
                    if (candle.Ema7 < candle.Ema10)
                    {
                        // 开仓
                        open = candle;
                        state = State.WaitingForShortClose;
                    }
                    break;

                case State.WaitingForShortClose:
                    if ((double)candle.HPrice > priceWhenXtoY)
                    {
                        close = candle;
                        double profile = ((double)open.CPrice - priceWhenXtoY) / (double)open.CPrice * 100;
                        totleProfile = totleProfile + profile;
                        Console.WriteLine($"开空:{open.DateTime:s}, 开仓价:{open.CPrice:f2}, 平空:{close.DateTime:s}, 平仓价{priceWhenXtoY:f2}, 盈亏{profile:f2}%");
                        if (candle.Ema7 > candle.Ema10)
                        {
                            open = candle;
                            state = State.WaitingForLongClose;
                        }
                        else
                        {
                            state = State.WaitingForLongOpen;
                        }
                    }
                    break;
                }

                prevCandle = candle;
            }
            Console.WriteLine($"总盈亏{totleProfile:f2}%");
        }

        private void AnalyzeImple1(IEnumerable<DataModels.Candle> candles)
        {
            int prevDirection = 0, prevPrevDirection = 0; ;
            double totleProfile = 0.0d;
            DataModels.Candle open = null, close = null;
            DataModels.Candle prevCandle = candles.First();
            State state = prevCandle.Ema7 > prevCandle.Ema10 ? State.WaitingForShortOpen : State.WaitingForLongOpen;
            foreach (DataModels.Candle candle in candles.Skip(1))
            {
                prevPrevDirection = prevDirection;
                prevDirection = candle.Ema7 > prevCandle.Ema7 ? 1 : -1;

                double priceWhenXtoY = MyTaLib.Ema.CalculatePriceWhenEmaXtoY(7, 10, prevCandle.Ema7, prevCandle.Ema10);
                switch (state)
                {
                case State.WaitingForLongOpen:
                    if (candle.Ema7 > candle.Ema10)
                    {
                        // 开多
                        open = candle;
                        state = State.WaitingForLongClose;
                    }
                    break;

                case State.WaitingForLongClose:
                    if ((double)candle.LPrice < priceWhenXtoY ||  (prevDirection < 0 && prevPrevDirection  < 0))
                    {
                        close = candle;
                        double closePrice = (double)candle.LPrice < priceWhenXtoY ? priceWhenXtoY : (double)close.CPrice;
                        double profile = (closePrice - (double)open.CPrice) / (double)open.CPrice * 100;
                        totleProfile = totleProfile + profile;
                        Console.WriteLine($"开多:{open.DateTime:s}, 开仓价:{open.CPrice:f2}, 平多:{close.DateTime:s}, 平仓价{closePrice:f2}, 盈亏{profile:f2}%");

                        if (candle.Ema7 < candle.Ema10)
                        {
                            open = candle;
                            state = State.WaitingForShortClose;
                        }
                        else
                        {
                            state = State.WaitingForShortOpen;
                        }
                    }
                    break;

                case State.WaitingForShortOpen:
                    if (candle.Ema7 < candle.Ema10)
                    {
                        // 开仓
                        open = candle;
                        state = State.WaitingForShortClose;
                    }
                    break;

                case State.WaitingForShortClose:
                    if ((double)candle.HPrice > priceWhenXtoY || (prevDirection > 0 && prevPrevDirection > 0))
                    {
                        close = candle;
                        double closePrice = (double)candle.HPrice > priceWhenXtoY ? priceWhenXtoY : (double)close.CPrice;
                        double profile = ((double)open.CPrice - closePrice) / (double)open.CPrice * 100;
                        totleProfile = totleProfile + profile;
                        Console.WriteLine($"开空:{open.DateTime:s}, 开仓价:{open.CPrice:f2}, 平空:{close.DateTime:s}, 平仓价{closePrice:f2}, 盈亏{profile:f2}%");
                        if (candle.Ema7 > candle.Ema10)
                        {
                            open = candle;
                            state = State.WaitingForLongClose;
                        }
                        else
                        {
                            state = State.WaitingForLongOpen;
                        }
                    }
                    break;
                }

                prevCandle = candle;
            }
            Console.WriteLine($"总盈亏{totleProfile:f2}%");
        }

        private void AnalyzeImple2(IEnumerable<DataModels.Candle> candles)
        {
            int prevDirection = 0, prevPrevDirection = 0; ;
            double totleProfile = 0.0d;
            DataModels.Candle open = null, close = null;
            DataModels.Candle prevCandle = candles.First();
            State state = prevCandle.Ema7 > prevCandle.Ema10 ? State.WaitingForShortOpen : State.WaitingForLongOpen;
            foreach (DataModels.Candle candle in candles.Skip(1))
            {
                prevPrevDirection = prevDirection;
                prevDirection = candle.Ema7 > prevCandle.Ema7 ? 1 : -1;

                switch (state)
                {
                case State.WaitingForLongOpen:
                    if (candle.Ema7 > candle.Ema10)
                    {
                        // 开多
                        open = candle;
                        state = State.WaitingForLongClose;
                    }
                    break;

                case State.WaitingForLongClose:
                    if (candle.Ema7 < candle.Ema10)
                    {
                        close = candle;
                        double profile = ((double)close.CPrice - (double)open.CPrice) / (double)open.CPrice * 100;
                        totleProfile = totleProfile + profile;
                        Console.WriteLine($"开多:{open.DateTime:s}, 开仓价:{open.CPrice:f2}, 平多:{close.DateTime:s}, 平仓价{close.CPrice:f2}, 盈亏{profile:f2}%");

                        open = candle;
                        state = State.WaitingForShortClose;
                    }
                    break;

                case State.WaitingForShortOpen:
                    if (candle.Ema7 < candle.Ema10)
                    {
                        // 开仓
                        open = candle;
                        state = State.WaitingForShortClose;
                    }
                    break;

                case State.WaitingForShortClose:
                    if (candle.Ema7 > candle.Ema10)
                    {
                        close = candle;
                        double profile = ((double)open.CPrice - (double)candle.CPrice) / (double)open.CPrice * 100;
                        totleProfile = totleProfile + profile;
                        Console.WriteLine($"开空:{open.DateTime:s}, 开仓价:{open.CPrice:f2}, 平空:{close.DateTime:s}, 平仓价{candle.CPrice:f2}, 盈亏{profile:f2}%");

                        open = candle;
                        state = State.WaitingForLongClose;
                    }
                    break;
                }

                prevCandle = candle;
            }
            Console.WriteLine($"总盈亏{totleProfile:f2}%");
        }

        private enum State
        {
            WaitingForLongOpen,
            WaitingForLongClose,
            WaitingForShortOpen,
            WaitingForShortClose,
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
