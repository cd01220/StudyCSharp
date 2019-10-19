using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Changes.ViewModels;
using Newtonsoft.Json;

namespace Changes
{
    public class OkexChanges
    {
        private const string BaseUrl = "https://www.okex.me";
        private const string FutureSegment = "api/futures/v3";
        private const string SwapSegment = "api/swap/v3";

        public async Task<List<Candle>> GetCandlesAsync(string contractId, DateTime start, DateTime end, CancellationToken token)
        {
            async Task<object> toTry() { return await GetCandlesImplAsync(contractId, start, end, token); }
            return (await Retry.RetryAsync(toTry, 5, token)) as List<Candle>;
        }

        public async Task<List<Candle>> GetCandlesImplAsync(string contractId, DateTime start, DateTime end, CancellationToken token)
        {
            string segment = contractId.IndexOf("SWAP") > 0 ? SwapSegment : FutureSegment;
            string url = $"{BaseUrl}/{segment}/instruments/{contractId}/candles";

            using (var delegatingHandler = new OkexDelegatingHandler(null))
            using (var httpClient = new HttpClient(delegatingHandler))
            using (var response = await httpClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                string contentStr = await response.Content.ReadAsStringAsync();

                OkexTicker ticker = JsonConvert.DeserializeObject<OkexTicker>(contentStr);
                return await Task.FromResult<List<Candle>>(new List<Candle>());
            }

            throw new InvalidOperationException("This should never happen!");
        }

        public async Task<Ticker> GetTickerAsync(string contractId, CancellationToken token)
        {
            async Task<object> toTry() { return await GetTickerImplAsync(contractId, token); }
            return (await Retry.RetryAsync(toTry, 5, token)) as Ticker;
        }

        private async Task<Ticker> GetTickerImplAsync(string contractId, CancellationToken token)
        {
            string segment = contractId.IndexOf("SWAP") > 0 ? SwapSegment : FutureSegment;
            string url = $"{BaseUrl}/{segment}/instruments/{contractId}/ticker";

            using (var delegatingHandler = new OkexDelegatingHandler(null))
            using (var httpClient = new HttpClient(delegatingHandler))
            using (var response = await httpClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                string contentStr = await response.Content.ReadAsStringAsync();

                OkexTicker ticker = JsonConvert.DeserializeObject<OkexTicker>(contentStr);
                return ticker;
            }

            throw new InvalidOperationException("This should never happen!");
        }
        
        private class OkexTicker: Ticker
        {
            [JsonProperty(PropertyName = "best_ask")]
            public override decimal AskPrice { get; set; }

            [JsonProperty(PropertyName = "best_bid")]
            public override decimal BidPrice { get; set; }

            [JsonProperty(PropertyName = "last")]
            public override decimal LastPrice { get; set; }

            // UTC 时间
            [JsonProperty(PropertyName = "timestamp")]
            public override DateTime DateTime { get; set; }
        }

        private class OkexCandle
        {
            [JsonProperty(PropertyName = "timestamp")]
            public virtual DateTime DateTime { get; set; }

            [JsonProperty(PropertyName = "open")]
            public virtual decimal OPrice { get; set; }

            [JsonProperty(PropertyName = "close")]
            public virtual decimal CPrice { get; set; }

            [JsonProperty(PropertyName = "high")]
            public virtual decimal HPrice { get; set; }

            [JsonProperty(PropertyName = "low")]
            public virtual decimal LPrice { get; set; }

            [JsonProperty(PropertyName = "volume")]
            public virtual decimal Volume { get; set; }
        }

        private class OkexDelegatingHandler : DelegatingHandler
        {
            private readonly ApiKey apiKey;
            private readonly string bodyStr;

            public OkexDelegatingHandler(ApiKey apiKey, string bodyStr)
            {
                this.apiKey = apiKey;
                this.bodyStr = bodyStr;

                InnerHandler = new HttpClientHandler();
            }

            public OkexDelegatingHandler(string bodyStr) : this(null, bodyStr)
            { }

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
}
