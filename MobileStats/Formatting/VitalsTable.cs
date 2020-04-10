using System;
using System.Collections.Generic;
using System.Linq;
using MobileStats.AppCenter;
using static MobileStats.AppCenter.AppNames;
using static MobileStats.TextAlignMode;

namespace MobileStats.Formatting
{
    static class VitalsTable
    {
        public static string Format(List<AppStatistics> apps, List<AppFigures.AppStatistics> appFiguresData)
        {
            var combinedData = apps.Select(a =>
                (a, appFiguresData.First(d => d.AppId == KnownAppNames[a.App].AppId))
            ).ToList();
            
            var kpis = new KPIExtractor();

            var tableFormatter = TableFormatter.WithColumns<(AppStatistics AppCenter, AppFigures.AppStatistics AppFigures)>(
                ("app", stats => formatName(stats.AppCenter), Left),
                ("stable ystday", stats => crashFreePercentage(stats.AppCenter, kpis.CrashfreeUsersOverLastFiveBuildsYesterday), Right),
                ("last 7 days", stats => crashFreePercentage(stats.AppCenter, kpis.CrashfreeUsersOverLastFiveBuildsLastWeek), Right),
                ("★★★★", stats => stats.AppFigures.RatingAverage.ToString("0.00"), Right),
                ("last 7d", stats => stats.AppFigures.RecentRatingAverage.ToString("0.00"), Right)
            );

            return tableFormatter.Format(combinedData);
        }

        private static string crashFreePercentage(AppStatistics stats, Func<List<AppVersionStatistics>, Percentage> percentage)
            => percentage(stats.VersionStatistics).ToStringWithConfidence();

        private static string formatName(AppStatistics stats)
            => KnownAppNames.TryGetValue(stats.App, out var name) ? name.Name : stats.App;

    }
}