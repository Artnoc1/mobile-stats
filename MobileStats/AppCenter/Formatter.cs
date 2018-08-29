using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MobileStats.TextAlignMode;
using VersionColumnSelector = System.ValueTuple<string, System.Func<MobileStats.AppCenter.AppVersionStatistics, string>, MobileStats.TextAlignMode>;
using AppColumnSelector = System.ValueTuple<string, System.Func<(MobileStats.AppCenter.AppStatistics, MobileStats.AppFigures.AppStatistics), string>, MobileStats.TextAlignMode>;

namespace MobileStats.AppCenter
{
    class Formatter
    {
        private static readonly Dictionary<string, (string Name, string Emoji, string AppId)> knownAppNames
            = new Dictionary<string, (string, string, string)>
            {
                ["Toggl-iOS"] = ("Daneel", ":daneel:", "com.toggl.daneel"),
                ["Toggl-Android"] = ("Giskard", ":giskard:", "com.toggl.giskard"),
            };

        public string FormatNameWithEmoji(AppStatistics stats)
        {
            if (knownAppNames.TryGetValue(stats.App, out var name))
            {
                return $"{name.Emoji} {name.Name}";
            }

            return stats.App;
        }
        public string FormatSummaryTable(List<AppStatistics> apps, List<AppFigures.AppStatistics> appFiguresData)
        {
            return "```\n" + tabelizeAppKPIs(apps, appFiguresData) + "```\n";
        }
        public string Format(List<AppVersionStatistics> statistics)
        {
            return "```\n" + tabelizeVersions(statistics) + "```\n";
        }

        private static string tabelizeAppKPIs(List<AppStatistics> apps, List<AppFigures.AppStatistics> appFiguresData)
        {
            var combinedData = apps.Select(a =>
                (a, appFiguresData.First(d => d.AppId == knownAppNames[a.App].AppId))
            ).ToList();

            var columns = appColumns();

            var rows = titleRowFrom(columns).Yield()
                .Concat(dataRowsFrom(combinedData, columns))
                .ToList();

            var rowWidths = Enumerable.Range(0, rows[0].Count)
                .Select(rowNumber => rows.Max(row => row[rowNumber].Length))
                .ToList();

            var rowAlignments = columns.Select(tuple => tuple.Item3).ToList();

            var builder = new StringBuilder();

            foreach (var row in rows)
                writeRow(builder, row, rowWidths, rowAlignments);

            return builder.ToString();
        }

        private static List<AppColumnSelector> appColumns()
            => new List<(string, Func<(AppStatistics AppCenter, AppFigures.AppStatistics AppFigures), object>, TextAlignMode)>
            {
                ("app", stats => formatName(stats.AppCenter), Left),
                ("w usrs", stats => stats.AppCenter.Totals.MostRecentWeeklyUsers, Right),
                ("stable ystday", stats =>
                {
                    var kpi = new KPIExtractor().CrashfreeUsersOverLastFiveBuildsYesterday(stats.AppCenter.VersionStatistics);
                    return formatPercentageWithConfidence(kpi.CrashFreePercentage, kpi.UsersConsidered);
                }, Right),
                ("last 7 days", stats =>
                {
                    var kpi = new KPIExtractor().CrashfreeUsersOverLastFiveBuildsLastWeek(stats.AppCenter.VersionStatistics);
                    return formatPercentageWithConfidence(kpi.CrashFreePercentage, kpi.UsersConsidered);
                }, Right),
                ("★★★★", stats => stats.AppFigures.RatingAverage.ToString("0.00"), Right),
                ("last 7d", stats => stats.AppFigures.RecentRatingAverage.ToString("0.00"), Right),
            }.Select<(string Title, Func<(AppStatistics, AppFigures.AppStatistics), object> GetStats, TextAlignMode TextAlign), AppColumnSelector>(
                t => (t.Title, stats => t.GetStats(stats).ToString(), t.TextAlign)).ToList();

        private static string formatName(AppStatistics stats)
            => knownAppNames.TryGetValue(stats.App, out var name) ? name.Name : stats.App;

        private static string tabelizeVersions(List<AppVersionStatistics> statistics)
        {
            var totalUsers = statistics.Sum(s => s.MostRecentDailyUsers);

            var shownStatistics = statistics
                .Where(s => s.MostRecentDailyUsers > 0)
                .OrderByDescending(s => s.Version.BuildNumber)
                .ToList();

            var columns = versionColumns(totalUsers);

            var rows = titleRowFrom(columns).Yield()
                .Concat(dataRowsFrom(shownStatistics, columns))
                .ToList();

            var rowWidths = Enumerable.Range(0, rows[0].Count)
                .Select(rowNumber => rows.Max(row => row[rowNumber].Length))
                .ToList();

            var rowAlignments = columns.Select(tuple => tuple.Item3).ToList();

            var builder = new StringBuilder();

            foreach (var row in rows)
                writeRow(builder, row, rowWidths, rowAlignments);

            return builder.ToString();
        }

        private static void writeRow(
            StringBuilder builder, List<string> cells,
            List<int> rowWidths, List<TextAlignMode> rowAlignments)
        {
            foreach (var (content, index) in cells.Select((c, i) => (content: c, index: i)))
            {
                builder.Append("| ");

                var alignLeft = rowAlignments[index] == Left;
                var paddedLength = rowWidths[index];
                var paddedContent = alignLeft
                    ? content.PadRight(paddedLength)
                    : content.PadLeft(paddedLength);

                builder.Append(paddedContent);
                builder.Append(' ');
            }
            builder.Append('|');
            builder.AppendLine();
        }

        private static List<string> titleRowFrom<T>(List<(string Title, T, TextAlignMode)> allDaysColumns)
            => allDaysColumns.Select(selector => selector.Title).ToList();

        private static IEnumerable<List<string>> dataRowsFrom<T>(List<T> apps, List<(string, Func<T, string> SelectValue, TextAlignMode)> columns)
            => apps.Select(s => columns.Select(selector => selector.SelectValue(s)).ToList());

        private static List<VersionColumnSelector> versionColumns(int totalMostRecentDailyUsers)
            => new List<(string title, Func<AppVersionStatistics, object> value, TextAlignMode alignment)>
            {
                ("version", stats => stats.Version.AppVersion, Left),
                ("users", stats => stats.MostRecentDailyUsers, Right),
                ("adoption", stats => formatPercentage(stats.MostRecentDailyUsers, totalMostRecentDailyUsers), Right),
                ("crash free", stats => formatPercentageWithConfidence(
                    stats.MostRecentCrashfreePercentage, stats.MostRecentDailyUsers), Right)
            }.Select<(string, Func<AppVersionStatistics, object>, TextAlignMode), VersionColumnSelector>(
                t => (t.Item1, app => t.Item2(app).ToString(), t.Item3)).ToList();

        public string FormatPercentageWithConfidence(double percentage, int total)
            => formatPercentageWithConfidence(percentage, total);

        private static string formatPercentageWithConfidence(double percentage, int total)
        {
            const double z = 1.96; // for 95% confidence interval

            var deviation = z * Math.Sqrt(percentage * (1 - percentage) / total);

            return $"{percentage * 100:0.00}% (±{deviation * 100:0.00}%)";
        }

        private static string formatPercentage(int count, int total)
        {
            var percentage = count * 100 / total;
            return percentage == 0 ? "<1%" : $"{percentage}%";
        }
    }
}
