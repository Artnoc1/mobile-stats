using System;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MobileStats.AppCenter.Api
{
    abstract class BaseApi
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

        private string url(string path)
            => $"https://api.appcenter.ms/v0.1/{path}";

        protected string url(string path, params (string name, string value)[] parameters)
            => parameters == null || parameters.Length == 0
                ? url(path)
                : url(path) + "?" + string.Join("&", parameters
                      .Where(p => p.value != null).Select(p => $"{p.name}={HttpUtility.UrlEncode(p.value)}")
                  );

        protected IObservable<T> RequestAndDeserialize<T>(string url)
            => Request(url)
                .SelectMany(r => r.Content.ReadAsStringAsync().ToObservable())
                .Select(Deserialize<T>);

        protected T Deserialize<T>(string json)
            => JsonConvert.DeserializeObject<T>(json, serializerSettings);

        protected IObservable<HttpResponseMessage> Request(string url)
            => httpClient().GetAsync(url).ToObservable().Select(
                response => response.IsSuccessStatusCode
                    ? response
                    : throw new Exception($"GET {url} returned with {(int)response.StatusCode} {response.StatusCode}: {response.Content.ReadAsStringAsync().Result}")
                );

        private HttpClient httpClient()
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("X-API-Token", authToken);
            return http;
        }
    }
}
