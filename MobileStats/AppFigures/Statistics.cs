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
            var aMonthAgo = today - TimeSpan.FromDays(30);
            
            var products = await api.Products();
            
            Console.WriteLine("Fetching numbers for apps: " + string.Join(", ", appids));

            var stats = new List<AppStatistics>();

            foreach (var product in products.Where(p => appids.Contains(p.BundleIdentifier)))
            {
                Console.WriteLine($"Fetching {product.BundleIdentifier}..");
                
                var ratings = await api.Ratings(aMonthAgo, today, product.Id, new []{"US"});
                
                stats.Add(new AppStatistics(product, ratings));
            }

            return stats;
        }
    }
}
