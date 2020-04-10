using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using MobileStats.AppCenter.Api;
using MobileStats.AppCenter.Models;

namespace MobileStats.AppCenter
{
    class Statistics
    {
        public static readonly ReadOnlyCollection<(string Event, string ShortName)> KeyEvents =
            new List<(string, string)>
            {
                ("SignUp", "signup"),
                ("Login", "login"),
                ("RatingViewWasShown", "rate view"),
                ("RatingViewSecondStepRate", "rated"),
                ("PushInitiatedSyncFetch", "PNs seen"),
                ("TimeEntryStarted", "new TEs"),
            }.AsReadOnly();
        
        private readonly string app;
        private readonly App api;

        public Statistics(string apiToken, string owner, string app)
        {
            api = new Api.AppCenter(apiToken).App(owner, app);
            this.app = app;
        }

        public async Task<AppStatistics> GetStatistics()
        {
            var midnight = new DateTimeOffset(DateTimeOffset.UtcNow.Date);
            var thirtyTwoDaysAgo = midnight.AddDays(-32);

            Console.WriteLine($"Getting app center statistics for {app}...");

            var versions = await api.Versions(thirtyTwoDaysAgo).ToTask();
            
            var versionStats = await Task.WhenAll(
                versions.Versions
                    .Select(async v => await getStatsFor(thirtyTwoDaysAgo, v))
                );

            var totals = await getStatsFor(thirtyTwoDaysAgo, null);

            var events = await Task.WhenAll(
                KeyEvents.Select(async e => (e.Event, await api.Event(e.Event).EventCount(thirtyTwoDaysAgo).ToTask()))
                );
            
            return new AppStatistics(app, totals, versionStats, events);
        }


        private async Task<AppVersionStatistics> getStatsFor(DateTimeOffset since, VersionCount version)
        {
            var friendlyVersionString = version == null ? "all versions" : $"{version.Version}";

            var versions = version == null ? null : new[] {version.Version};

            Console.WriteLine($"Getting numbers for {friendlyVersionString}");
            var activeDevices = await api.ActiveDeviceCounts(since, versions: versions).ToTask();
            var crashfreePercentages = await api.CrashfreeDevicePercentages(since, versions: versions).ToTask();
            // not currently used for anything
            //var crashCounts = await appApi.CrashCounts(since, versions: versions).ToTask();

            Console.WriteLine($"Crunching numbers for {friendlyVersionString}");
            return new AppVersionStatistics(version, activeDevices, crashfreePercentages, null);
        }
    }
}
