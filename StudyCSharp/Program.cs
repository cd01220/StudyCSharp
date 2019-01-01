using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Reactive.Testing;
using Newtonsoft.Json;

namespace StudyCSharp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Subject<int> subject = new Subject<int>();
            var subscription0 = subject.Subscribe(
                                     x => Console.WriteLine("subscription0 Value published: {0}", x),
                                     () => Console.WriteLine("Sequence Completed."));
            var subscription1 = subject.Subscribe(
                                     x => Console.WriteLine("subscription1 Value published: {0}", x),
                                     () => Console.WriteLine("Sequence Completed."));

            subject.OnNext(1);

            subject.OnNext(2);

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
            subject.OnCompleted();
            subscription0.Dispose();
            subscription1.Dispose();
        }
    }
}
