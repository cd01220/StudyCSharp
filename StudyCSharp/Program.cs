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
            RxPractices.TestRx1();
            Console.ReadKey();            
        }
    }
}
