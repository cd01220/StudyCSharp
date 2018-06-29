﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StudyCSharp
{
    public class MarketEnt
    {
        public decimal Price { get; set; }
    }

    public class MarketPoller : IObservable<MarketEnt>
    {
        public delegate void UnsubscribeHandler(IObserver<MarketEnt> observer);

        private object syncObj;
        private List<IObserver<MarketEnt>> observers;

        private Thread thread;
        private CancellationTokenSource cts;

        public MarketPoller()
        {
            this.syncObj = new object();
            this.observers = new List<IObserver<MarketEnt>>();
        }

        public IDisposable Subscribe(IObserver<MarketEnt> observer)
        {
            lock (this.syncObj)
            {
                if (this.observers.Count == 0)
                {
                    this.cts = new CancellationTokenSource();
                    this.thread = new Thread(this.ThreadProc);
                    this.thread.Start();
                }

                if (!this.observers.Contains(observer))
                {
                    this.observers.Add(observer);
                }
            }

            return new Unsubscriber(this.Unsubscribe, observer);
        }

        private void Unsubscribe(IObserver<MarketEnt> observer)
        {
            lock (this.syncObj)
            {
                this.observers.Remove(observer);

                if (this.observers.Count == 0)
                {
                    this.cts.Cancel();
                    this.thread.Join();
                }
            }
        }

        private void ThreadProc()
        {
            while (!this.cts.Token.IsCancellationRequested)
            {
                MarketEnt marketEnt = new MarketEnt();

                /* Unsubscribe() 可能拿到了同一个锁 syncObj, 并且在 join 当前线程.
                 * 为了避免死锁, 为lock设定一个超时的时间.
                 */
                if (Monitor.TryEnter(this.syncObj, TimeSpan.FromMilliseconds(100)))
                {
                    if (marketEnt == null)
                    {
                        foreach (IObserver<MarketEnt> observer in observers)
                        {
                            observer.OnError(new ApplicationException("HTTP timeout"));
                        }
                    }
                    else
                    {
                        foreach (IObserver<MarketEnt> observer in observers)
                        {
                            observer.OnNext(marketEnt);
                        }
                    }
                    Monitor.Exit(this.syncObj);
                }

                Thread.Sleep(1000);
            }
        }

        private class Unsubscriber : IDisposable
        {
            private readonly UnsubscribeHandler unsubscribeHandler;
            private readonly IObserver<MarketEnt> observer;

            internal Unsubscriber(UnsubscribeHandler unsubscribeHandler, IObserver<MarketEnt> observer)
            {
                this.unsubscribeHandler = unsubscribeHandler;
                this.observer = observer;
            }

            public void Dispose()
            {
                this.unsubscribeHandler(this.observer);
            }
        }
    }

    public class Robot : IObserver<MarketEnt>
    {
        private IDisposable unsubscriber;
        public Robot() { }

        public void Start()
        {
            MarketPoller marketPoller = new MarketPoller();
            this.unsubscriber = marketPoller.Subscribe(this);
        }

        public void Stop()
        {
            this.unsubscriber.Dispose();
        }

        public void OnCompleted()
        {
            // Do nothing.
        }

        public void OnError(Exception error)
        {
            // Do nothing.
            Console.WriteLine((error as ApplicationException).Message);
        }

        public void OnNext(MarketEnt marketEnt)
        {
            Console.WriteLine("ask price={0:0.00}", marketEnt.Price);
        }
    }

    public class ObserverPrictice
    {
        public static void TestObserverPrictice()
        {
            TestObserverPrictice01();
        }

        public static void TestObserverPrictice01()
        {
            Robot robot = new Robot();
            robot.Start();

            Console.ReadKey();

            robot.Stop();
        }
    }
}
