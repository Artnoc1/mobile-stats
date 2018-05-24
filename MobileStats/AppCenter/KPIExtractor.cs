using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileStats.AppCenter
{
    class KPIExtractor
    {
        public (double CrashFreePercentage, int UsersConsidered)
            CrashfreeUsersOverLastFiveBuildsYesterday(List<AppVersionStatistics> statistics)
        {
            var relevantVersions = lastFiveVersions(statistics);

            var totalUsers = relevantVersions.Sum(s => s.MostRecentDailyUsers);

            var weightedCrashfreeUsersSum = relevantVersions
                .Sum(s => s.MostRecentCrashfreePercentage * s.MostRecentDailyUsers);

            return (weightedCrashfreeUsersSum / totalUsers, totalUsers);
        }

        public (double CrashFreePercentage, int UsersConsidered)
            CrashfreeUsersOverLastFiveBuildsLastWeek(List<AppVersionStatistics> statistics)
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

            return (weightedCrashfreeUsersSum / totalUsers, totalUsers);
        }

        private static List<AppVersionStatistics> lastFiveVersions(List<AppVersionStatistics> statistics)
            => statistics
                .OrderByDescending(s => s.Version.BuildNumber)
                .Take(5)
                .ToList();
    }
}
