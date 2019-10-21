using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StudyCSharp
{
    // 参考资料： 
    // 1： https://stackoverflow.com/questions/10838847/track-the-number-of-observers-in-an-observable
    // 2： https://social.msdn.microsoft.com/forums/en-US/2805dd0d-764a-4669-876d-18e7f4c2828c/multicast-vs-publish
    class RxPractices
    {
        private static long DoSomething(long x)
        {
            Console.WriteLine("{0}", x);
            return x;
        }

        // 如果没有observers， 停止远程查询（这里用假设DoSomething()查询远程服务器的信息）。 实现方案1，通过RefCount / Publish实现无需对
        // observers计数。
        public static void TestRx1()
        {
            IObservable<long> observable = Observable.Interval(TimeSpan.FromSeconds(1))
                .Select(x => DoSomething(x));

            IObservable<long> multicast = observable.Publish().RefCount();

            IDisposable sub0 = multicast.Subscribe(x => Console.WriteLine($"sub0: {x}"));
            IDisposable sub1 = multicast.Subscribe(x => Console.WriteLine($"sub1: {x}"));
        }

        public static IObservable<long> CountSubscribers()
        {
            int count = 0;
            IObservable<long> source = Observable.Interval(TimeSpan.FromSeconds(1));

            IObservable<long> observable = Observable.Defer(() =>
            {
                count = Interlocked.Increment(ref count);
                if (count == 1)
                {
                    // start task;
                }
                Console.WriteLine($"count = {count}");
                return source;
            });

            return observable.Finally(() =>
            {
                count = Interlocked.Decrement(ref count);
                if (count == 0)
                { }
                Console.WriteLine($"count = {count}");
            });
        }

        // 如果没有observers， 停止远程查询（这里用假设DoSomething()查询远程服务器的信息）。 实现方案2，利用Defer/Finally 对observer计数。
        public static void TestRx2()
        {
            var source = CountSubscribers();
            IDisposable sub0 = source.Subscribe(x => Console.WriteLine($"sub0: {x}"));
            IDisposable sub1 = source.Subscribe(x => Console.WriteLine($"sub1: {x}"));

            Console.ReadKey();
            sub0.Dispose();
            sub1.Dispose();
        }
    }
}
