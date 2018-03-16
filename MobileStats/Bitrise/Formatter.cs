using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MobileStats.Bitrise.TextAlignMode;
using ColumnSelector = System.ValueTuple<string, System.Func<MobileStats.Bitrise.AppBuildStatistics, string>, MobileStats.Bitrise.TextAlignMode>;

namespace MobileStats.Bitrise
{
    class Formatter
    {
        public string Format(List<AppBuildStatistics> statistics)
        {
            return "```\n" + tabelize(statistics) + "```\n";
        }

        private static string tabelize(List<AppBuildStatistics> statistics)
        {
            var days = statistics[0].TotalDays;
            var initialColumn = new List<ColumnSelector>
            {
                ("app", app => app.Name, Left),
            };
            var allDaysColumns = initialColumn
                .Concat(buildCollectionColumns(app => app.TotalStats))
                .ToList();
            var lastDayColumns = initialColumn
                .Concat(buildCollectionColumns(app => app.LastDayStats))
                .ToList();

            var rows = titleRowFrom(allDaysColumns).Yield()
                .Concat(dataRowsFrom(statistics, allDaysColumns))
                .Concat(dataRowsFrom(statistics, lastDayColumns))
                .ToList();

            var rowWidths = Enumerable.Range(0, rows[0].Count)
                .Select(rowNumber => rows.Max(row => row[rowNumber].Length))
                .ToList();

            var rowAlignments = allDaysColumns.Select(tuple => tuple.Item3).ToList();
            
            var builder = new StringBuilder();

            writeRow(builder, rows[0], rowWidths, rowAlignments);

            builder.Append($"last {days} days:\n");
            foreach (var i in Enumerable.Range(1, statistics.Count))
                writeRow(builder, rows[i], rowWidths, rowAlignments);

            builder.Append("last 24 hours:\n");
            foreach (var i in Enumerable.Range(1 + statistics.Count, statistics.Count))
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

        private static IEnumerable<List<string>> dataRowsFrom(List<AppBuildStatistics> apps, List<ColumnSelector> columns)
            => apps.Select(s => columns.Select(selector => selector.Item2(s)).ToList());

        private static List<ColumnSelector> buildCollectionColumns(Func<AppBuildStatistics, BuildCollectionStatistics> getBuilds)
            => new List<(string title, Func<BuildCollectionStatistics, object> value)>
            {
                (":.", builds => builds.TotalCount),
                ("✓", builds => $"{100 * builds.SuccessfulCount / builds.FinishedCount}%"),
                ("run μ", builds => formatDuration(builds.SuccessfulWorkingDurationAverage)),
                ("σ", builds => formatDuration(builds.SuccessfulWorkingDurationSTD)),
                ("wait μ", builds => formatDuration(builds.PendingDurationAverage)),
                ("σ", builds => formatDuration(builds.PendingDurationSTD)),
                ("95%", builds => formatDuration(builds.PendingDuration95Percent)),
            }.Select<(string, Func<BuildCollectionStatistics, object>), ColumnSelector>(
                t => (t.Item1, app => t.Item2(getBuilds(app)).ToString(), Right)).ToList();

        private static string formatDuration(TimeSpan t)
            => t > TimeSpan.FromMinutes(1)
                ? $"{t.TotalMinutes:0.0}m"
                : $"{t.TotalSeconds:0}s";
    }
}
