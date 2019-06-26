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
        private static readonly HttpClient client = new HttpClient();

        public static void TestWebClient()
        {
            TestWebClient05().Wait();
        }

        /// <summary>
        /// the simplest way to get ticket information from okex.com;
        /// </summary>
        /// <returns></returns>
        public static async Task TestWebClient01()
        {
            string uri = "https://www.okex.com/api/v1/future_ticker.do?symbol=btc_usd&contract_type=this_week";            
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 10; ++i)
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    string responseBody = await client.GetStringAsync(uri);
                    Console.WriteLine(responseBody);

                    TickerContainer tickerContainer = JsonConvert.DeserializeObject<TickerContainer>(responseBody, UnixSecondsConverter.Instance);
                    Console.WriteLine(JsonConvert.SerializeObject(tickerContainer, Formatting.Indented, UnixSecondsConverter.Instance));
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
        /// get ticket from okex.com with cancellation support, and basic error management
        /// </summary>
        /// <returns></returns>
        public static async Task TestWebClient02() 
        {
            string uri = "https://www.okex.com/api/v1/future_ticker.do?symbol=btc_usd&contract_type=this_week";
            CancellationTokenSource cts = new CancellationTokenSource(); 
            
            // Call asynchronous network methods in a try/catch block to handle exceptions
            try
            {
                HttpResponseMessage response = await client.GetAsync(uri, cts.Token);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                TickerContainer tickerContainer = JsonConvert.DeserializeObject<TickerContainer>(responseBody, UnixSecondsConverter.Instance);
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        /// <summary>
        /// get ticket from okex.com with cancellation support, and custom exception.
        /// </summary>
        /// <returns></returns>
        public static async Task TestWebClient03() 
        {
            string uri = "https://www.okex.com/api/v1/future_ticker.do?symbol=btc_usd&contract_type=this_week";
            CancellationTokenSource cts = new CancellationTokenSource();

            // Call asynchronous network methods in a try/catch block to handle exceptions
            try
            {
                HttpResponseMessage response = await client.GetAsync(uri, cts.Token);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("something is wrong.");
                }
                string responseBody = await response.Content.ReadAsStringAsync();

                TickerContainer tickerContainer = JsonConvert.DeserializeObject<TickerContainer>(responseBody, UnixSecondsConverter.Instance);
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        private static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            StreamReader streamReader = null;
            T searchResult = default(T);
            try 
            {
                streamReader = new StreamReader(stream);
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    streamReader = null;
                    var settings = new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { new UnixSecondsConverter() }
                    };
                    var jsonSerializer = JsonSerializer.Create(settings);
                    searchResult = jsonSerializer.Deserialize<T>(jsonTextReader);
                }
            }
            finally
            {
                if (streamReader != null)
                    streamReader.Dispose();

                stream = null;
            }

            return searchResult;
        }

        private static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
            {
                using (var streamReader = new StreamReader(stream))
                {
                    content = await streamReader.ReadToEndAsync();
                }
            }

            return content;
        }

        /// <summary>
        /// get ticket from okex.com with cancellation support, custom exception, and using streams to save memory.
        /// </summary>
        /// <returns></returns>
        public static async Task TestWebClient04()
        {
            string uri = "https://www.okex.com/api/v1/future_ticker.do?symbol=btc_usd&contract_type=this_week";            
            CancellationTokenSource cts = new CancellationTokenSource();

            // Call asynchronous network methods in a try/catch block to handle exceptions
            try
            {
                HttpResponseMessage response = await client.GetAsync(uri, cts.Token);

                // to avoid CA2202 warning (https://msdn.microsoft.com/library/ms182334.aspx)
                // using try statement, instead of using (stream = await response.Content.ReadAsStreamAsync()) {...}
                Stream stream = null;
                try
                {
                    stream = await response.Content.ReadAsStreamAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await StreamToStringAsync(stream);
                        throw new HttpRequestException(responseBody);
                    }

                    var tickerContainer = DeserializeJsonFromStream<TickerContainer>(stream);
                    Console.WriteLine(JsonConvert.SerializeObject(tickerContainer, Formatting.Indented, UnixSecondsConverter.Instance));
                }
                catch(HttpRequestException e)
                {
                    throw e;
                }
                finally
                {
                    if (stream != null)
                        stream.Dispose();
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            Debugger.Break();
        }

        /// <summary>
        /// http client with DelegatingHandler
        /// </summary>
        /// <returns></returns>
        public static async Task TestWebClient05()
        {
            string uri = "https://www.okex.com/api/v1/future_ticker.do?symbol=btc_usd&contract_type=next_week";
            Stopwatch sw = Stopwatch.StartNew();

            OkexDelegatingHandler delegatingHandler = new OkexDelegatingHandler(null);
            HttpClient httpClient = new HttpClient(delegatingHandler);
            // Call asynchronous network methods in a try/catch block to handle exceptions
            try
            {                
                string responseBody = await httpClient.GetStringAsync(uri);
                Console.WriteLine(responseBody);

                TickerContainer tickerContainer = JsonConvert.DeserializeObject<TickerContainer>(responseBody, UnixSecondsConverter.Instance);
                Console.WriteLine(JsonConvert.SerializeObject(tickerContainer, Formatting.Indented, UnixSecondsConverter.Instance));
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

    public class OkexDelegatingHandler : DelegatingHandler
    {
        private readonly string publicKey;
        private readonly string privateKey;
        private readonly string passPhrase;
        private readonly string bodyStr;

        public OkexDelegatingHandler(string publicKey, string privateKey, string passPhrase, string bodyStr)
        {
            this.publicKey = publicKey;
            this.privateKey = privateKey;
            this.passPhrase = passPhrase;
            this.bodyStr = bodyStr;

            InnerHandler = new HttpClientHandler();
        }

        public OkexDelegatingHandler(string bodyStr)
        {
            this.bodyStr = bodyStr;

            InnerHandler = new HttpClientHandler();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string timeStamp = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrEmpty(publicKey))
            {
                Debug.Assert(string.IsNullOrEmpty(privateKey) && string.IsNullOrEmpty(passPhrase));
                string method = request.Method.Method;
                request.Headers.Add("OK-ACCESS-KEY", publicKey);

                var requestUrl = request.RequestUri.PathAndQuery;
                string sign = "";
                if (!string.IsNullOrEmpty(bodyStr))
                {
                    sign = HmacSHA256($"{timeStamp}{method}{requestUrl}{bodyStr}", privateKey);
                }
                else
                {
                    sign = HmacSHA256($"{timeStamp}{method}{requestUrl}", privateKey);
                }

                request.Headers.Add("OK-ACCESS-SIGN", sign);
                request.Headers.Add("OK-ACCESS-PASSPHRASE", passPhrase);
            }
            else
            {
                Debug.Assert(string.IsNullOrEmpty(privateKey) && string.IsNullOrEmpty(passPhrase));
            }

            request.Headers.Add("OK-ACCESS-TIMESTAMP", timeStamp.ToString());

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
