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
        private readonly DateTime tomorrow;

        public Statistics(string userName, string password, string clientKey, params string[] appids)
        {
            this.appids = appids;
            api = new Api.AppFigures(userName, password, clientKey);

            tomorrow = DateTime.UtcNow.AddHours(12);
        }

        public async Task<List<AppStatistics>> FetchStats()
        {
            var products = await api.Products();
            
            Console.WriteLine("Fetching numbers for apps: " + string.Join(", ", appids));

            var stats = new List<AppStatistics>();

            foreach (var product in products.Where(p => appids.Contains(p.BundleIdentifier)))
            {
                Console.WriteLine($"Fetching {product.BundleIdentifier}..");
                
                var ratings = await api.Ratings(tomorrow - TimeSpan.FromDays(8), tomorrow, product.Id, null);
                
                stats.Add(new AppStatistics(product, ratings));
            }

            return stats;
        }
    }
}
