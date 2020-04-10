using System.Collections.Generic;
using MobileStats.AppCenter;
using static MobileStats.AppCenter.AppNames;
using static MobileStats.TextAlignMode;

namespace MobileStats.Formatting
{
    static class ActiveUsersTable
    {
        public static string Format(List<AppStatistics> apps)
        {
            var table = TableFormatter.WithColumns<AppStatistics>(
                ("app", formatName, Left),
                ("DAU", a => a.Totals.MostRecentDailyUsers, Right),
                ("vs -1d", a => formatUserChange(a.Totals.MostRecentDailyUsersChange), Right),
                ("vs -7d", a => formatUserChange(a.Totals.MostRecentDailyUsersChangeWeekly), Right),
                ("WAU", a => a.Totals.MostRecentWeeklyUsers, Right),
                ("vs -1w", a => formatUserChange(a.Totals.MostRecentWeeklyUsersChange), Right),
                ("MAU", a => a.Totals.MostRecentMonthlyUsers, Right),
                ("vs -1m", a => formatUserChange(a.Totals.MostRecentMonthlyUsersChange), Right)
            );

            return table.Format(apps);
        }
        
        private static string formatName(AppStatistics stats)
            => KnownAppNames.TryGetValue(stats.App, out var name) ? name.Name : stats.App;
        
        private static string formatUserChange(Percentage change)
            => $"{change.Fraction * 100 - 100:+0.0;-0.0}%";
    }
}