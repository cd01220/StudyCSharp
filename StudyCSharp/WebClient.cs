namespace StudyCSharp
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class WebClient
    {
        public static void TestWebClient()
        {
            TestWebClient01().Wait();
            Debugger.Break();
        }

        public static async Task TestWebClient01()
        {
            string url = "https://www.okex.com/api/v1/future_ticker.do?symbol=btc_usd&contract_type=this_week";
            // Create a New HttpClient object.
            using (HttpClient client = new HttpClient())
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Above three lines can be replaced with new helper method below
                    // string responseBody = await client.GetStringAsync(uri);

                    Console.WriteLine(responseBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            } //using (HttpClient client = new HttpClient())
        }
    } //public class WebClient

    public class Ticker
    {
        public decimal High { get; set; }
    }
}
