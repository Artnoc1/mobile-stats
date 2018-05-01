using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using MobileStats.Bitrise.Models;
using App = MobileStats.Bitrise.Api.App;

namespace MobileStats.Bitrise
{
    class Statistics
    {
        const int daysToFetch = 7;

        private readonly DateTimeOffset now = DateTimeOffset.UtcNow;
        private readonly string apiToken;
        private readonly string[] appIds;
        private readonly DateTimeOffset cutoffDate;

        public Statistics(string apiToken, params string[] appIds)
        {
            this.apiToken = apiToken;
            this.appIds = appIds;

            cutoffDate = now - TimeSpan.FromDays(daysToFetch);
        }

        public async Task<List<AppBuildStatistics>> GetStatistics()
        {
            Console.WriteLine("Bitrise apps: " + string.Join(", ", appIds));

            var apps = await fetchAllApps();

            foreach (var app in apps)
                Console.WriteLine($"{app.Slug} is {app.Title}");

            Console.WriteLine($"Fetching builds until {cutoffDate}...");

            var builds = await fetchAllBuilds(apps);

            var totalBuildCount = builds.Sum(list => list.Count);

            Console.WriteLine($"Fetched {totalBuildCount} builds in total");
            Console.WriteLine("Doing the math...");

            var appsWithTitle = apps.Select(a => a.Title)
                .Zip(builds, (t, b) => (t, b));

            if (apps.Length > 1)
                appsWithTitle = appsWithTitle
                    .Concat(new[] {("Total", builds.SelectMany(b => b).ToList())});

            var stats = appsWithTitle
                .Select(b => new AppBuildStatistics(b.Item1, daysToFetch, now, b.Item2))
                .ToList();

            return stats;
        }

        private Task<Models.App[]> fetchAllApps()
            => Task.WhenAll(appApis.Select(api => api.Info().ToTask()));
        
        private Task<List<Build>[]> fetchAllBuilds(Models.App[] apps)
            => Task.WhenAll(appApis.Zip(apps, (api, app) => buildsForApp(api, app.Title)));

        private IEnumerable<App> appApis => appIds.Select(apiForApp);

        private App apiForApp(string id) => new Api.Bitrise(apiToken).App(id);

        private async Task<List<Build>> buildsForApp(App app, string title)
        {
            var builds = new List<Build>();
            var nextPage = (string)null;

            while (true)
            {
                var page = await app.Builds(nextPage);
                var oldestBuild = page.Data.Min(b => b.TriggeredAt);

                var buildsInRange = page.Data.Where(b => b.TriggeredAt >= cutoffDate).ToList();

                Console.WriteLine($"Fetched {page.Data.Count} builds for {title}, the oldest being from {oldestBuild}, keeping {buildsInRange.Count}");

                builds.AddRange(buildsInRange);
                nextPage = page.Paging.Next;
                
                if (nextPage == null || buildsInRange.Count != page.Data.Count)
                {
                    break;
                }
            }

            return builds;
        }
    }
}
