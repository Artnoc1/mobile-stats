using System.Collections.Generic;
using System.Linq;
using MobileStats.Formatting;

namespace MobileStats.AppCenter
{
    class KPIExtractor
    {
        public Percentage CrashfreeUsersOverLastFiveBuildsYesterday(List<AppVersionStatistics> statistics)
        {
            var relevantVersions = lastFiveVersions(statistics);

            var totalUsers = relevantVersions.Sum(s => s.MostRecentDailyUsers);

            var weightedCrashfreeUsersSum = relevantVersions
                .Sum(s => s.MostRecentCrashfreePercentage * s.MostRecentDailyUsers);

            return Percentage.FromFraction(weightedCrashfreeUsersSum / totalUsers, totalUsers);
        }

        public Percentage CrashfreeUsersOverLastFiveBuildsLastWeek(List<AppVersionStatistics> statistics)
        {
            var relevantVersions = lastFiveVersions(statistics);

            var (weightedCrashfreeUsersSum, totalUsers) = relevantVersions.Aggregate(
                (weightedAverageSum: 0.0, totalUsers: 0),
                (aggregate, stats) =>
                {
                    foreach (var datedPercentage in stats.CrashfreePercentages.DailyPercentages.Take(7))
                    {
                        var devices = stats.ActiveDevices.Daily
                            .Single(d => d.Datetime == datedPercentage.Datetime);

                        aggregate.weightedAverageSum += datedPercentage.Percentage * devices.Count;
                        aggregate.totalUsers += devices.Count;
                    }

                    return aggregate;
                });

            return Percentage.FromFraction(weightedCrashfreeUsersSum / totalUsers, totalUsers);
        }

        private static List<AppVersionStatistics> lastFiveVersions(List<AppVersionStatistics> statistics)
            => statistics
                .OrderAlphanumericallyDescendingBy(s => s.Version.Version)
                .Take(5)
                .ToList();
    }
}
