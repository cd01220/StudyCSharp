using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Changes;
using Changes.ViewModels;
using Mapster;

namespace StudyCSharp
{
    public class EmaTrading
    {
        public EmaTrading()
        { }

        public void Analyze()
        {
            int n1 = 7;
            int n2 = 10;
            int granularity = 3600 * 1;
            DateTime beginTime = DateTime.Parse("2019-09-11T16:00:00Z").ToUniversalTime();
            DateTime endTime = DateTime.Parse("2019-10-22T00:00:00Z").ToUniversalTime();
            // 测试BTC季度合约，EMA7 EMA10 交叉时买入卖出。 Get
            IEnumerable<DataModels.Candle> btcCandles = GetCandles("BTC-USD-191227", granularity, beginTime, endTime);
            //Console.WriteLine(JsonConvert.SerializeObject(btcCandles, Formatting.Indented));
            Console.WriteLine($"=============BTC {n1}, {n2}=============");
            AnalyzeImple(btcCandles, n1, n2, 10045.68d, 10051.38d);

            // 测试ETH季度合约，EMA7 EMA10 交叉时买入卖出。
            IEnumerable<DataModels.Candle> ethCandles = GetCandles("ETH-USD-191227", granularity, beginTime, endTime);
            //Console.WriteLine(JsonConvert.SerializeObject(ethCandles, Formatting.Indented));
            Console.WriteLine($"=============ETH {n1}, {n2}=============");
            AnalyzeImple(ethCandles, n1, n2, 177.534d, 177.975d);

            //测试EOS季度合约，EMA7 EMA10 交叉时买入卖出。            
            IEnumerable<DataModels.Candle> eosCandles = GetCandles("EOS-USD-191227", granularity, beginTime, endTime);
            //Console.WriteLine(JsonConvert.SerializeObject(eosCandles, Formatting.Indented));
            Console.WriteLine($"=============EOS0 {n1}, {n2}=============");
            AnalyzeImple(eosCandles, n1, n2, 3.709d, 3.715d);
        }

        private void AnalyzeImple(IEnumerable<DataModels.Candle> candles, int n1, int n2, double firstN1Ema, double firstN2Ema)
        {
            double totlePercentage = 0.0d;
            int right = 0;
            int wrong = 0;
            DataModels.Candle open = null, close = null;

            (double n1Ema, double n2Ema) = (firstN1Ema, firstN2Ema);
            State state = n1Ema > n2Ema ? State.WaitingForShortOpen : State.WaitingForLongOpen;
            foreach (DataModels.Candle candle in candles.Skip(1))
            {
                n1Ema = MyTaLib.Ema.CalculateEma(n1, n1Ema, (double)candle.CPrice);
                n2Ema = MyTaLib.Ema.CalculateEma(n2, n2Ema, (double)candle.CPrice);

                switch (state)
                {
                case State.WaitingForLongOpen:
                    if (n1Ema > n2Ema)
                    {
                        // 开多
                        open = candle;
                        state = State.WaitingForLongClose;
                    }
                    break;

                case State.WaitingForLongClose:
                    if (n1Ema < n2Ema)
                    {
                        close = candle;
                        double percentage = ((double)close.CPrice - (double)open.CPrice) / (double)open.CPrice * 100;
                        totlePercentage = totlePercentage + percentage;
                        if (percentage > 0.0d)
                            right++;
                        else
                            wrong++;
                        Console.WriteLine($"开多:{open.DateTime:s}, 开仓价:{open.CPrice:f2}, 平多:{close.DateTime:s}, 平仓价:{close.CPrice:f2}, 盈亏:{percentage:f2}%");

                        open = candle;
                        state = State.WaitingForShortClose;
                    }
                    break;

                case State.WaitingForShortOpen:
                    if (n1Ema < n2Ema)
                    {
                        // 开仓
                        open = candle;
                        state = State.WaitingForShortClose;
                    }
                    break;

                case State.WaitingForShortClose:
                    if (n1Ema > n2Ema)
                    {
                        close = candle;
                        double percentage = ((double)open.CPrice - (double)candle.CPrice) / (double)open.CPrice * 100;
                        totlePercentage = totlePercentage + percentage;
                        if (percentage > 0.0d)
                            right++;
                        else
                            wrong++;
                        Console.WriteLine($"开空:{open.DateTime:s}, 开仓价:{open.CPrice:f2}, 平空:{close.DateTime:s}, 平仓价:{candle.CPrice:f2}, 盈亏:{percentage:f2}%");

                        open = candle;
                        state = State.WaitingForLongClose;
                    }
                    break;
                }
            }
            Console.WriteLine($"总盈亏:{totlePercentage:f2}%, 正确:{right}, 错误:{wrong}");
        }

        private IEnumerable<DataModels.Candle> GetCandles(string Id, int granularity, DateTime beginTime, DateTime endTime)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            OkexChanges changes = new OkexChanges();

            // 转换 UTC 时间为北京时间。
            TypeAdapterConfig typeAdapterConfig = new TypeAdapterConfig();
            typeAdapterConfig.ForType<Candle, DataModels.Candle>().Map(dst => dst.DateTime, src => src.DateTime.ToLocalTime());

            List<DataModels.Candle> candles = new List<DataModels.Candle>();
            while (beginTime < endTime)
            {
                IEnumerable<DataModels.Candle> cs = changes.GetCandlesAsync(Id, granularity, beginTime, endTime, tokenSource.Token)
                .Result
                .Select(o => o.Adapt<DataModels.Candle>(typeAdapterConfig))
                .ToList();

                candles.AddRange(cs);
                beginTime = beginTime.AddHours(cs.Count() * 2);
            }

            return candles;
        }

        private enum State
        {
            WaitingForLongOpen,
            WaitingForLongClose,
            WaitingForShortOpen,
            WaitingForShortClose,
        }
    }
}
