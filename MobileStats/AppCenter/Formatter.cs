using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using static MobileStats.TextAlignMode;
using ColumnSelector = System.ValueTuple<string, System.Func<MobileStats.AppCenter.AppVersionStatistics, string>, MobileStats.TextAlignMode>;

namespace MobileStats.AppCenter
{
    class Formatter
    {
        public string Format(List<AppVersionStatistics> statistics)
        {
            return "```\n" + tabelize(statistics) + "```\n";
        }

        private static string tabelize(List<AppVersionStatistics> statistics)
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

            foreach (var i in Enumerable.Range(0, shownStatistics.Count + 1))
                writeRow(builder, rows[i], rowWidths, rowAlignments);

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

        private static List<string> titleRowFrom(List<ColumnSelector> allDaysColumns)
            => allDaysColumns.Select(selector => selector.Item1).ToList();

        private static IEnumerable<List<string>> dataRowsFrom(List<AppVersionStatistics> apps, List<ColumnSelector> columns)
            => apps.Select(s => columns.Select(selector => selector.Item2(s)).ToList());

        private static List<ColumnSelector> versionColumns(int totalMostRecentDailyUsers)
            => new List<(string title, Func<AppVersionStatistics, object> value, TextAlignMode alignment)>
            {
                ("version", stats => stats.Version.AppVersion, Left),
                ("users", stats => stats.MostRecentDailyUsers, Right),
                ("adoption", stats => formatPercentage(stats.MostRecentDailyUsers, totalMostRecentDailyUsers), Right),
                ("crash free", stats => formatPercentageWithConfidence(
                    stats.MostRecentCrashfreePercentage, stats.MostRecentDailyUsers), Right)
            }.Select<(string, Func<AppVersionStatistics, object>, TextAlignMode), ColumnSelector>(
                t => (t.Item1, app => t.Item2(app).ToString(), t.Item3)).ToList();

        private static string formatPercentage(int count, int total)
        {
            var percentage = count * 100 / total;
            return percentage == 0 ? "<1%" : $"{percentage}%";
        }

        private static string formatPercentageWithConfidence(double percentage, int total)
        {
            const double z = 1.96; // for 95% confidence interval

            var deviation = z * Math.Sqrt(percentage * (1 - percentage) / total);

            return $"{percentage * 100:0.00}% (±{deviation * 100:0.00}%)";
        }
    }
}
