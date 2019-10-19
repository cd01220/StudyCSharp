using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Changes.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Changes
{
    public class OkexChanges
    {
        private const string BaseUrl = "https://www.okex.me";
        private const string FutureSegment = "api/futures/v3";
        private const string SwapSegment = "api/swap/v3";

        public async Task<List<Candle>> GetCandlesAsync(string contractId, int granularity, DateTime? start, DateTime? end, CancellationToken token)
        {
            async Task<object> toTry() { return await GetCandlesImplAsync(contractId, granularity, start, end, token); }
            return (await Retry.RetryAsync(toTry, 5, token)) as List<Candle>;
        }

        public async Task<List<Candle>> GetCandlesImplAsync(string contractId, int granularity, DateTime? start, DateTime? end, CancellationToken token)
        {
            string segment = contractId.IndexOf("SWAP") > 0 ? SwapSegment : FutureSegment;
            string url = $"{BaseUrl}/{segment}/instruments/{contractId}/candles";

            using (var delegatingHandler = new OkexDelegatingHandler(null))
            using (var httpClient = new HttpClient(delegatingHandler))
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                if (start.HasValue)
                {
                    queryParams.Add("start", start.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                }
                if (end.HasValue)
                {
                    queryParams.Add("end", end.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                }
                queryParams.Add("granularity", granularity.ToString());
                using (var encodedContent = new FormUrlEncodedContent(queryParams))
                {
                    string paramsStr = await encodedContent.ReadAsStringAsync();
                    using (var response = await httpClient.GetAsync($"{url}?{paramsStr}"))
                    {
                        response.EnsureSuccessStatusCode();
                        string contentStr = await response.Content.ReadAsStringAsync();

                        List<Candle> candles = new List<Candle>();
                        JArray jArray = JArray.Parse(contentStr);
                        foreach (JToken v in jArray)
                        {
                            string[] iterms = v.ToArray().Select(o => o.ToString()).ToArray();
                            Candle candle = new Candle()
                            {
                                DateTime = DateTime.Parse(iterms[0]),
                                OPrice = decimal.Parse(iterms[1]),
                                HPrice = decimal.Parse(iterms[2]),
                                LPrice = decimal.Parse(iterms[3]),
                                CPrice = decimal.Parse(iterms[4]),
                                Volume = decimal.Parse(iterms[5])
                            };
                            candles.Insert(0, candle);
                        }
                        return candles;
                    }
                }
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
