using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MobileStats.AppFigures
{
    class Statistics
    {
        private readonly string[] appids;
        private readonly Api.AppFigures api;
        private readonly DateTime today;

        public Statistics(string userName, string password, string clientKey, params string[] appids)
        {
            this.appids = appids;
            api = new Api.AppFigures(userName, password, clientKey);

            today = DateTime.UtcNow;
        }

        public async Task<List<AppStatistics>> FetchStats()
        {
            var products = await api.Products();
            var ratings = await api.Ratings(today, today);

            Console.WriteLine("Crunching numbers for apps: " + string.Join(", ", appids));

            return products
                .Select(p => new AppStatistics(p, ratings.First(r => r.Product == p.Id)))
                .Where(s => appids.Contains(s.AppId))
                .ToList();
        }
    }
}
