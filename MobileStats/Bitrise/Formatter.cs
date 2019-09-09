﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MobileStats.TextAlignMode;
using ColumnSelector = System.ValueTuple<string, System.Func<MobileStats.Bitrise.WorkflowBuildStatistics, string>, MobileStats.TextAlignMode>;

namespace MobileStats.Bitrise
{
    class Formatter
    {
        public string Format(List<WorkflowBuildStatistics> statistics)
        {
            return "```\n" + tabelize(statistics) + "```\n";
        }

        private static string tabelize(List<WorkflowBuildStatistics> statistics)
        {
            var days = statistics[0].TotalDays;
            var initialColumn = new List<ColumnSelector>
            {
                ("workflow", getWorkflowName, Left),
            };
            var allDaysColumns = initialColumn
                .Concat(buildCollectionColumns(workflow => workflow.TotalStats))
                .ToList();
            var lastDayColumns = initialColumn
                .Concat(buildCollectionColumns(workflow => workflow.LastDayStats))
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

        private static string getWorkflowName(WorkflowBuildStatistics w)
        {
            const int maxLength = 24;

            if (w.Name.Length < maxLength)
                return w.Name;

            var components = w.Name.Split('.');
            var lenghts = components.Select(c => c.Length).ToList();

            var charsToRemove = w.Name.Length - maxLength;

            foreach (var _ in Enumerable.Range(0, charsToRemove))
            {
                var currentMaxLength = lenghts.Max();
                if (currentMaxLength == 2)
                    break;
                
                var currentMaxLengthIndex = lenghts.IndexOf(currentMaxLength);

                lenghts[currentMaxLengthIndex]--;
            }

            return string.Join(".",
                components.Zip(lenghts, (c, l) =>
                    c.Length == l
                        ? c
                        : $"{c.Substring(0, l - 1)}~"
                ));
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

        private static IEnumerable<List<string>> dataRowsFrom(List<WorkflowBuildStatistics> workflows, List<ColumnSelector> columns)
            => workflows.Select(s => columns.Select(selector => selector.Item2(s)).ToList());

        private static IEnumerable<ColumnSelector> buildCollectionColumns(Func<WorkflowBuildStatistics, BuildCollectionStatistics> getBuilds)
            => new (string title, Func<BuildCollectionStatistics, object> value)[]
            {
                (":.", builds => builds.TotalCount),
                ("✓", builds => formatPercentage(builds.SuccessfulCount, builds.FinishedCount)),
                ("run μ", builds => formatDuration(builds.SuccessfulWorkingDurationAverage)),
                ("σ", builds => formatDuration(builds.SuccessfulWorkingDurationSTD)),
                ("wait μ", builds => formatDuration(builds.PendingDurationAverage)),
                ("σ", builds => formatDuration(builds.PendingDurationSTD)),
                ("95%", builds => formatDuration(builds.PendingDuration95Percent)),
            }.Select<(string, Func<BuildCollectionStatistics, object>), ColumnSelector>(
                t => (t.Item1, app => t.Item2(getBuilds(app)).ToString(), Right));

        private static string formatPercentage(int count, int total)
            => total == 0
                ? "N/A"
                : $"{100 * count / total}%";

        private static string formatDuration(TimeSpan t)
            => t > TimeSpan.FromMinutes(1)
                ? $"{t.TotalMinutes:0.0}m"
                : $"{t.TotalSeconds:0}s";
    }
}
