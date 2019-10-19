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
            string str0 = "hello";
            string str1 = null;
            string v = $"{str0}{str1}";

            WebClient.TestWebClient();
        }
    }
}
