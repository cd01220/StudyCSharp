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
using System.Windows.Forms;
using Changes;
using Microsoft.Reactive.Testing;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StudyCSharp
{
    public class Program
    {   
        private static void Main(string[] args)
        {
            var tokenSource = new CancellationTokenSource(); 

            OkexChanges changes = new OkexChanges();
            var result = changes.GetTickerAsync("BTC-USD-191227", tokenSource.Token).Result;
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}
