namespace StudyCSharp
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    public class TaskBasedAsynchronousPattern
    {
        public static void TestAsynchronous()
        {
            TestAsynchronous04();
            TestAsynchronous04();

            Console.ReadKey();
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

        public static async void TestAsynchronous03()
        {
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            BroadcastBlock<int> broadcast = new BroadcastBlock<int>(v => v);
            BufferBlock<int> bufferBlock0 = new BufferBlock<int>();
            BufferBlock<int> bufferBlock1 = new BufferBlock<int>();

            IDisposable unlink0 = broadcast.LinkTo(bufferBlock0, linkOptions);
            IDisposable unlink1 = broadcast.LinkTo(bufferBlock1, linkOptions);

            broadcast.Post(1);
            broadcast.Post(2);

            Console.WriteLine("bufferBlock0 {0}", await bufferBlock0.ReceiveAsync());
            Console.WriteLine("bufferBlock0 {0}", await bufferBlock0.ReceiveAsync());

            Console.WriteLine("bufferBlock1 {0}", await bufferBlock1.ReceiveAsync());
            Console.WriteLine("bufferBlock1 {0}", await bufferBlock1.ReceiveAsync());

            unlink0.Dispose();
            unlink1.Dispose();

            broadcast.Complete();            
        }

        public static async void TestAsynchronous04()
        {
            for (int i = 0; i < 10; ++i)
            {
                MarketEnt market = await GetMarket();
                Console.WriteLine("{0}, {1}", market.Time, market.Last);
            }
        }

        private static BufferBlock<MarketEnt> bufferBlock = new BufferBlock<MarketEnt>();
        private static object syncObj = new object();
        private static bool running = false;
        private static async Task<MarketEnt> GetMarket()
        {
            lock (syncObj)
            {
                if (bufferBlock.Count == 0 && !running)
                {
                    // Refresh
                }
            }
            return await bufferBlock.ReceiveAsync();
        }
    }
}
