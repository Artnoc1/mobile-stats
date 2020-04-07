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

        public Statistics(IConfiguration config)
        {
            var user = config.AppFiguresUser;
            var password = config.AppFiguresPassword;
            var clientKey = config.AppFiguresClientKey;
            
            api = new Api.AppFigures(user, password, clientKey);
            appids = config.AppFiguresProductIds.Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            tomorrow = DateTime.UtcNow.AddHours(12);
        }

        public async Task<List<AppStatistics>> FetchStats()
        {
            Console.WriteLine("Fetching app figures statistics...");
            
            var products = await api.Products();
            
            Console.WriteLine("Fetching numbers for apps: " + string.Join(", ", appids));

            var stats = new List<AppStatistics>();

            foreach (var product in products.Where(p => appids.Contains(p.BundleIdentifier)))
            {
                Console.WriteLine($"Fetching {product.BundleIdentifier}..");
                
                var ratings = await api.Ratings(tomorrow - TimeSpan.FromDays(8), tomorrow, product.Id, null);
                
                stats.Add(new AppStatistics(product, ratings));
            }

            Console.WriteLine("Finished fetching app figures statistics");

            return stats;
        }
    }
}
