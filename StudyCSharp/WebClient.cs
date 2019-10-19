using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StudyCSharp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class WebClient
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static void TestWebClient()
        {
            TestWebClient02().Wait();
        }

        /// <summary>
        /// the simplest way to get ticket information from okex.com;
        /// </summary>
        /// <returns></returns>
        public static async Task TestWebClient01()
        {
            string url = "https://www.okex.me/api/futures/v3/instruments/BTC-USD-191227/ticker";
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 10; ++i)
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    string jsonString = await httpClient.GetStringAsync(url);
                    Console.WriteLine(jsonString);

                    OkexContractMarket okexContractMarket = JsonConvert.DeserializeObject<OkexContractMarket>(jsonString);
                    Console.WriteLine(JsonConvert.SerializeObject(okexContractMarket, Formatting.Indented));
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            sw.Stop();
            Console.WriteLine("Elapsed = " + sw.ElapsedMilliseconds);
            Debugger.Break();
        }

        /// <summary>
        /// http client with DelegatingHandler
        /// </summary>
        /// <returns></returns>
        public static async Task TestWebClient02()
        {
            string url = "https://www.okex.me/api/futures/v3/instruments/BTC-USD-191227/ticker";

            Stopwatch sw = Stopwatch.StartNew();

            OkexDelegatingHandler delegatingHandler = new OkexDelegatingHandler(null);
            HttpClient httpClient = new HttpClient(delegatingHandler);
            // Call asynchronous network methods in a try/catch block to handle exceptions
            try
            {
                // var response = await httpClient.GetAsync(url);

                string jsonString = await httpClient.GetStringAsync(url);
                Console.WriteLine(jsonString);

                OkexContractMarket okexContractMarket = JsonConvert.DeserializeObject<OkexContractMarket>(jsonString);
                Console.WriteLine(JsonConvert.SerializeObject(okexContractMarket, Formatting.Indented));
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            sw.Stop();
            Console.WriteLine("Elapsed = " + sw.ElapsedMilliseconds);
            Debugger.Break();
        }
    } //public class WebClient   

    public class ApiKey
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string Passphrase { get; set; }
    }

    public class OkexDelegatingHandler : DelegatingHandler
    {
        private readonly ApiKey apiKey;
        private readonly string bodyStr;

        public OkexDelegatingHandler(ApiKey apiKey, string bodyStr)
        {
            this.apiKey = apiKey;
            this.bodyStr = bodyStr;

            InnerHandler = new HttpClientHandler();
        }

        public OkexDelegatingHandler(string bodyStr): this(null, bodyStr)
        {}

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string timeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!(apiKey is null))
            {
                string sign = HmacSHA256($"{timeStamp}{request.Method.Method}{request.RequestUri.PathAndQuery}{bodyStr}", apiKey.PrivateKey);

                request.Headers.Add("OK-ACCESS-KEY", apiKey.PublicKey);
                request.Headers.Add("OK-ACCESS-SIGN", sign);
                request.Headers.Add("OK-ACCESS-PASSPHRASE", apiKey.Passphrase);
            }

            request.Headers.Add("OK-ACCESS-TIMESTAMP", timeStamp);
            return base.SendAsync(request, cancellationToken);
        }

        private string HmacSHA256(string infoStr, string secret)
        {
            byte[] sha256Data = Encoding.UTF8.GetBytes(infoStr);
            byte[] secretData = Encoding.UTF8.GetBytes(secret);
            using (var hmacsha256 = new HMACSHA256(secretData))
            {
                byte[] buffer = hmacsha256.ComputeHash(sha256Data);
                return Convert.ToBase64String(buffer);
            }
        }
    }
}
