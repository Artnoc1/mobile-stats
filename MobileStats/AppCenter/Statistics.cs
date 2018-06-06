using System;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using MobileStats.AppCenter.Api;
using MobileStats.AppCenter.Models;

namespace MobileStats.AppCenter
{
    class Statistics
    {
        private readonly string apiToken;
        private readonly string owner;
        private readonly string app;

        public Statistics(string apiToken, string owner, string app)
        {
            this.apiToken = apiToken;
            this.owner = owner;
            this.app = app;
        }

        public async Task<AppStatistics> GetStatistics()
        {
            var api = new Api.AppCenter(apiToken);

            var appApi = api.App(owner, app);

            var midnight = new DateTimeOffset(DateTimeOffset.UtcNow.Date);
            var sevenDaysAgo = midnight.AddDays(-7);

            Console.WriteLine($"Getting app center statistics for {app}...");

            var versions = await appApi.Versions().ToTask();

            var versionStats = await Task.WhenAll(
                versions
                    .GroupBy(v => v.AppVersion)
                    .Select(vs => vs.OrderByDescending(v => v.BuildNumber).First())
                    .Select(async v => await getStatsFor(appApi, sevenDaysAgo, v))
                );

            var totals = await getStatsFor(appApi, sevenDaysAgo, null);

            return new AppStatistics(app, totals, versionStats);
        }

        private async Task<AppVersionStatistics> getStatsFor(App appApi, DateTimeOffset since, VersionInfo version)
        {
            var friendlyVersionString = version == null ? "all versions" : $"{version.AppVersion}";

            var versions = version == null ? null : new[] {version.AppVersion};

            Console.WriteLine($"Getting numbers for {friendlyVersionString}");
            var activeDevices = await appApi.ActiveDeviceCounts(since, versions: versions).ToTask();
            var crashfreePercentages = await appApi.CrashfreeDevicePercentages(since, versions: versions).ToTask();
            var crashCounts = await appApi.CrashCounts(since, versions: versions).ToTask();

            Console.WriteLine($"Crunching numbers for {friendlyVersionString}");
            return new AppVersionStatistics(version, activeDevices, crashfreePercentages, crashCounts);
        }
    }
}
