using System.Collections.Generic;
using System.Linq;
using MobileStats.AppCenter.Models;

namespace MobileStats.AppCenter
{
    class AppStatistics
    {
        public AppVersionStatistics Totals { get; }

        public List<AppVersionStatistics> VersionStatistics { get; }

        public AppStatistics(AppVersionStatistics totals,
            IEnumerable<AppVersionStatistics> versionStatistics)
        {
            Totals = totals;
            VersionStatistics = versionStatistics?.ToList() ?? new List<AppVersionStatistics>();
        }
    }
}
