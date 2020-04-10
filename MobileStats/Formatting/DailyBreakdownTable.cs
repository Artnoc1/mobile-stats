using System.Collections.Generic;
using System.Linq;
using MobileStats.AppCenter;
using static MobileStats.AppCenter.AppNames;

namespace MobileStats.Formatting
{
    static class DailyBreakdownTable
    {
        
        public static string Format(List<AppStatistics> appCenterData)
            => string.Join("", appCenterData.Select(s =>
                $"{FormatNameWithEmoji(s)}\n{Format(s.VersionStatistics)}"
            ));

        public static string FormatNameWithEmoji(AppStatistics stats)
        {
            if (KnownAppNames.TryGetValue(stats.App, out var name))
            {
                return $"{name.Emoji} {name.Name}";
            }

            return stats.App;
        }

        public static string Format(List<AppVersionStatistics> statistics)
        {
            var shownStatistics = statistics
                .Where(s => s.MostRecentDailyUsers > 0)
                .OrderAlphanumericallyDescendingBy(a => a.Version.Version)
                .ToList();
            
            var totalUsers = shownStatistics.Sum(s => s.MostRecentDailyUsers);

            var tableFormatter = TableFormatter.WithColumns<AppVersionStatistics>(
                ("version", stats => stats.Version.Version, TextAlignMode.Left),
                ("users", stats => stats.MostRecentDailyUsers, TextAlignMode.Right),
                ("adoption", stats => Percentage.FromSamples(stats.MostRecentDailyUsers, totalUsers), TextAlignMode.Right),
                ("crash free", stats => Percentage.FromFraction(stats.MostRecentCrashfreePercentage, stats.MostRecentDailyUsers)
                    .ToStringWithConfidence(), TextAlignMode.Right)
                );

            return tableFormatter.Format(shownStatistics);
        }
    }
}
