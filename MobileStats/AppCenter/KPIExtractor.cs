using System.Collections.Generic;
using System.Linq;

namespace MobileStats.AppCenter
{
    class KPIExtractor
    {
        public (double CrashFreePercentage, int UsersConsidered) CrashfreeUsersOverLastFiveBuilds(List<AppVersionStatistics> statistics)
        {
            var relevantVersions = statistics
                .OrderByDescending(s => s.Version.BuildNumber)
                .Take(5)
                .ToList();

            var totalUsers = relevantVersions.Sum(s => s.MostRecentDailyUsers);

            var weightedCrashfreeUsersSum = relevantVersions
                .Sum(s => s.MostRecentCrashfreePercentage * s.MostRecentDailyUsers);

            return (weightedCrashfreeUsersSum / totalUsers, totalUsers);
        }
    }
}
