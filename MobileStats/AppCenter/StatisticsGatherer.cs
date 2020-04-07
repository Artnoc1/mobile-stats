using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MobileStats.AppCenter
{
    class StatisticsGatherer
    {
        private readonly string apiToken;
        private readonly string owner;
        private readonly string[] apps;

        public StatisticsGatherer(IConfiguration config)
        {
            apiToken = config.AppCenterApiToken;
            owner = config.AppCenterOwner;
            apps = config.AppCenterApps.Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }
        
        
        public async Task<List<AppStatistics>> FetchAll()
        {
            Console.WriteLine("Fetching app center statistics...");

            var stats = new List<AppStatistics>();

            foreach (var app in apps)
            {
                Console.WriteLine($"Fetching app center statistics for {app}...");
                stats.Add(await new Statistics(apiToken, owner, app).GetStatistics());
            }

            Console.WriteLine("Finished fetching app center statistics");

            return stats;
        }
    }
}