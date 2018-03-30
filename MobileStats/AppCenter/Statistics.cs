using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

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
            var activeDevices = await appApi.ActiveDeviceCounts(sevenDaysAgo).ToTask();
            var crashfreePercentages = await appApi.CrashfreeDevicePercentages(sevenDaysAgo).ToTask();
            var crashCounts = await appApi.CrashCounts(sevenDaysAgo).ToTask();

            Console.WriteLine("Crunching app center numbers...");
            var totals = new AppVersionStatistics(activeDevices, crashfreePercentages, crashCounts);

            return new AppStatistics(totals, null);
        }
    }
}
