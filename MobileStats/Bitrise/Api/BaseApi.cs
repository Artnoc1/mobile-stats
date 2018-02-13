using System;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MobileStats.Bitrise.Api
{
    class BaseApi
    {
        private readonly string authToken;

        public BaseApi(string authToken)
        {
            this.authToken = authToken;
        }

        private static readonly JsonSerializerSettings serializerSettings =
            new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

        protected string url(string path)
            => $"https://api.bitrise.io/v0.1/{path}";


        protected IObservable<T> RequestAndDeserialize<T>(string url)
            => Request(url)
                .SelectMany(r => r.Content.ReadAsStringAsync().ToObservable())
                .Select(Deserialize<T>);

        protected T Deserialize<T>(string json)
            => JsonConvert.DeserializeObject<T>(json, serializerSettings);

        protected IObservable<HttpResponseMessage> Request(string url)
            => httpClient().GetAsync(url).ToObservable();

        private HttpClient httpClient()
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Authorization", authToken);
            return http;
        }
    }
}
