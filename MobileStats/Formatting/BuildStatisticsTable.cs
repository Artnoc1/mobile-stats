using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileStats.Bitrise;

namespace MobileStats.Formatting
{
    static class BuildStatisticsTable
    {
        public static string Format(List<WorkflowBuildStatistics> statistics)
        {
            return "```\n" + tabelize(statistics) + "```\n";
        }

        private static string tabelize(List<WorkflowBuildStatistics> statistics)
        {
            var tableFormatter = TableFormatter.WithColumns<(WorkflowBuildStatistics w, BuildCollectionStatistics b)>(
                ("workflow", x => getWorkflowName(x.w), TextAlignMode.Left),
                ("#", x => x.b.TotalCount, TextAlignMode.Right),
                ("✓", x => formatPercentage(x.b.SuccessfulCount, x.b.FinishedCount), TextAlignMode.Right),
                ("run μ", x => formatDuration(x.b.SuccessfulWorkingDurationAverage), TextAlignMode.Right),
                ("σ", x => formatDuration(x.b.SuccessfulWorkingDurationSTD), TextAlignMode.Right),
                ("wait μ", x => formatDuration(x.b.PendingDurationAverage), TextAlignMode.Right),
                ("σ", x => formatDuration(x.b.PendingDurationSTD), TextAlignMode.Right),
                ("95%", x => formatDuration(x.b.PendingDuration95Percent), TextAlignMode.Right)
                );

            var allDaysRows = statistics.Select(w => (w, w.TotalStats));
            var lastDayRows = statistics.Select(w => (w, w.LastDayStats));
            
            var lines = tableFormatter.FormatLines(allDaysRows.Concat(lastDayRows));

            var builder = new StringBuilder();
            
            builder.AppendLine(lines[0]);

            builder.Append($"last {statistics[0].TotalDays} days:\n");
            foreach (var i in Enumerable.Range(1, statistics.Count))
                builder.AppendLine(lines[i]);

            builder.Append("last 24 hours:\n");
            foreach (var i in Enumerable.Range(1 + statistics.Count, statistics.Count))
                builder.AppendLine(lines[i]);

            return builder.ToString();
        }

        private static string getWorkflowName(WorkflowBuildStatistics w)
        {
            const int maxLength = 20;
            
            
            var name = w.App == null
                ? w.Name
                : $"{w.App.Title.Substring(0, 3)}.{w.Name}";
            
            if (name.Length < maxLength)
                return name;

            var components = name.Split('.', '-');
            var lengths = components.Select(c => c.Length).ToList();

            var charsToRemove = name.Length - maxLength;

            foreach (var _ in Enumerable.Range(0, charsToRemove))
            {
                var currentMaxLength = lengths.Max();
                if (currentMaxLength == 2)
                    break;
                
                var currentMaxLengthIndex = lengths.IndexOf(currentMaxLength);

                lengths[currentMaxLengthIndex]--;
            }

            return string.Join(".",
                components.Zip(lengths, (c, l) =>
                    c.Length == l
                        ? c
                        : $"{c.Substring(0, l - 1)}~"
                ));
        }

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
