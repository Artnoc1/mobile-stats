using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Web;
using MobileStats.AppFigures.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MobileStats.AppFigures.Api
{
    class AppFigures
    {
        private readonly string userName;
        private readonly string password;
        private readonly string clientKey;

        public AppFigures(string userName, string password, string clientKey)
        {
            this.userName = userName;
            this.password = password;
            this.clientKey = clientKey;
        }

        public IObservable<List<Rating>> Ratings(DateTime startDate, DateTime endDate)
        {
            return requestAndDeserialize<List<Rating>>(
                url("ratings",
                    ("start_date", date(startDate)),
                    ("end_date", date(endDate))
                    ));
        }

        public IObservable<List<Product>> Products()
        {
            return requestAndDeserialize<Dictionary<string, Product>>(
                url("products/mine")
            ).Select(d => d.Values.ToList());
        }

        private static string date(DateTime date)
            => $"{date.Year:0000}-{date.Month:00}-{date.Day:00}";

        private static readonly JsonSerializerSettings serializerSettings =
            new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

        private IObservable<T> requestAndDeserialize<T>(string url)
            => request(url)
                .SelectMany(r => r.Content.ReadAsStringAsync().ToObservable())
                .Select(deserialize<T>);

        private T deserialize<T>(string json)
            => JsonConvert.DeserializeObject<T>(json, serializerSettings);

        private IObservable<HttpResponseMessage> request(string url)
            => httpClient().GetAsync(url).ToObservable();

        protected string url(string path, params (string name, string value)[] parameters)
            => parameters == null || parameters.Length == 0
                ? url(path)
                : url(path) + "?" + string.Join("&", parameters
                      .Where(p => p.value != null).Select(p => $"{p.name}={HttpUtility.UrlEncode(p.value)}")
                  );

        private string url(string path)
            => $"https://api.appfigures.com/v2/{path}";

        private HttpClient httpClient()
        {
            var http = new HttpClient();
            var base64Auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));
            http.DefaultRequestHeaders.Add("Authorization", $"Basic {base64Auth}");
            http.DefaultRequestHeaders.Add("X-Client-Key", clientKey);
            return http;
        }

    }
}
