using System.Collections.Generic;
using System.Linq;

namespace MobileStats.AppCenter
{
    class AppStatistics
    {
        public AppVersionStatistics Totals { get; }

        public List<(string Version, AppVersionStatistics Statistics)> VersionStatistics { get; }

        public AppStatistics(AppVersionStatistics totals,
            IEnumerable<(string version, AppVersionStatistics statistics)> versionStatistics)
        {
            Totals = totals;
            VersionStatistics = versionStatistics?.ToList() ?? new List<(string, AppVersionStatistics)>();
        }
    }
}