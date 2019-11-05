using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StudyCSharp.DataModels;

namespace Common
{
    public class Program
    {
        private static void Main()
        {
            var candle = new Candle { DateTime = DateTime.Now, CPrice = 1.01m, HPrice = 1.02m, LPrice = 1.0m };
            Console.WriteLine("candle = {0}", JsonConvert.SerializeObject(candle));
        }
    }
}
