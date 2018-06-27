namespace StudyCSharp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class WebClient
    {
        private static readonly HttpClient client = new HttpClient();

        public static void TestWebClient()
        {
            TestWebClient04().Wait();
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
    } //public class WebClient
}
