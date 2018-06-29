namespace StudyCSharp
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class TaskBasedAsynchronousPattern
    {
        public static void TestAsynchronous()
        {
            var result = TestAsynchronous02().Result;

            Console.WriteLine(string.Empty);
        }

        public static void TestAsynchronous01()
        {
            TaskCompletionSource<int> tcs1 = new TaskCompletionSource<int>();
            Task<int> t1 = tcs1.Task;

            Action action = delegate ()
            {
                Thread.Sleep(1000);
                tcs1.SetResult(15);
            };

            // Start a background task that will complete tcs1.Task
            Task.Factory.StartNew(action);

            // The attempt to get the result of t1 blocks the current thread until the completion source gets signaled.
            // It should be a wait of ~1000 ms.
            Stopwatch sw = Stopwatch.StartNew();
            int result = t1.Result;
            sw.Stop();

            Console.WriteLine("(ElapsedTime={0}): t1.Result={1} (expected 15) ", sw.ElapsedMilliseconds, result);

            // ------------------------------------------------------------------
            // Alternatively, an exception can be manually set on a TaskCompletionSource.Task
            TaskCompletionSource<int> tcs2 = new TaskCompletionSource<int>();
            Task<int> t2 = tcs2.Task;

            // Start a background Task that will complete tcs2.Task with an exception
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                tcs2.SetException(new InvalidOperationException("SIMULATED EXCEPTION"));
            });

            // The attempt to get the result of t2 blocks the current thread until the completion source gets signaled with either a result or an exception.
            // In either case it should be a wait of ~1000 ms.
            sw = Stopwatch.StartNew();
            try
            {
                result = t2.Result;
                sw.Stop();

                Console.WriteLine("t2.Result succeeded. THIS WAS NOT EXPECTED.");
            }
            catch (AggregateException e)
            {
                if (sw.IsRunning)
                    sw.Stop();
                Console.Write("(ElapsedTime={0}): ", sw.ElapsedMilliseconds);
                Console.WriteLine("The following exceptions have been thrown by t2.Result: (THIS WAS EXPECTED)");
                for (int j = 0; j < e.InnerExceptions.Count; j++)
                {
                    Console.WriteLine("\n-------------------------------------------------\n{0}", e.InnerExceptions[j].ToString());
                }
            }
        }

        //#pragma warning disable 1998
        private static async Task<int> ReadAsync()
        {
            int hours = 0;
            await Task.Delay(2000);

            return hours;
        }

        public static Task<int> TestAsynchronous02()
        {
            Task<int> task =  ReadAsync();
            
            return task;
        }


        // 模拟btc spot-fututures-arbitrage robot的主线程：监听最新的期货和现货价格，Robot根据最新的价格做相应的处理，并继续监听价格变化。
        public static void TestAsynchronous03()
        {
            Changes changes = new Changes();

            Action action = delegate ()
            {
                for (int i = 0; i < 100; ++i)
                {
                    Task<decimal> price0 = changes.GetPrice(0);
                    Task<decimal> price1 = changes.GetPrice(1);

                    Task.WhenAny(price0, price1);
                }

                return;
            };

            Task.Run(action);

            Console.ReadKey();
            return ;
        }

        public class Changes
        {
            // 同一个平台的行情信息可能被多个robot使用。
            public async Task<decimal> GetPrice(int currency)
            {
                await Task.Delay(2000);
                return 0.0M;
            }
        }
    }
}
