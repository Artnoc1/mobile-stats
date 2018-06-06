using System.Collections.Generic;
using System.Linq;
using MobileStats.AppCenter.Models;

namespace MobileStats.AppCenter
{
    class AppStatistics
    {
        public string App { get; }

        public AppVersionStatistics Totals { get; }

        public List<AppVersionStatistics> VersionStatistics { get; }

        public AppStatistics(string app, AppVersionStatistics totals,
            IEnumerable<AppVersionStatistics> versionStatistics)
        {
            App = app;
            Totals = totals;
            VersionStatistics = versionStatistics?.ToList() ?? new List<AppVersionStatistics>();
        }
    }
}
