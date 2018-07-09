using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MobileStats.TextAlignMode;

namespace MobileStats.Code
{
    public class Formatter
    {
        public string FormatKPIs(List<Folder> folders)
        {
            var totalLOCs = folders.Sum(f => f.LinesOfCode);
            var testLOCs = folders.Where(f => f.Name.Contains("Tests")).Sum(f => f.LinesOfCode);

            var daneelLOCs = folders.Single(f => f.Name == "Daneel").LinesOfCode;
            var giskardLOCs = folders.Single(f => f.Name == "Giskard").LinesOfCode;

            var sharedCodeProjects = new HashSet<string>
            {
                "Multivac",
                "Ultrawave",
                "PrimeRadiant",
                "PrimeRadiant.Realm",
                "Foundation",
                "Foundation.MvvmCross",
            };

            var sharedCodeLOCs = folders.Where(f => sharedCodeProjects.Contains(f.Name)).Sum(f => f.LinesOfCode);

            var totalDaneelLOCs = daneelLOCs + sharedCodeLOCs;
            var totalGiskardLOCs = giskardLOCs + sharedCodeLOCs;

            var rows = new List<string[]>
            {
                new [] {"group", "locs", "%" },
                new [] {"tests", testLOCs.ToString("N"), $"{formatPercentage(testLOCs, totalLOCs)} of total"},
                new [] {"shared code", sharedCodeLOCs.ToString("N"), ""},
                new [] {"daneel total", totalDaneelLOCs.ToString("N"), $"{formatPercentage(sharedCodeLOCs, totalDaneelLOCs)} is shared"},
                new [] {"giskard total", totalGiskardLOCs.ToString("N"), $"{formatPercentage(sharedCodeLOCs, totalGiskardLOCs)} is shared"},
            };

            var rowWidths = Enumerable.Range(0, rows[0].Length)
                .Select(rowNumber => rows.Max(row => row[rowNumber].Length))
                .ToList();

            var rowAlignments = new [] { Left, Right, Right};

            var builder = new StringBuilder();

            foreach (var row in rows)
                writeRow(builder, row, rowWidths, rowAlignments);

            return "```\n" + builder + "```\n";
        }

        public string FormatLinesOfCode(List<Folder> folders)
        {
            return "```\n" + tabelizeFolders(folders) + "```\n";
        }

        private static string tabelizeFolders(List<Folder> folders)
        {
            var totalLinesOfCode = folders.Sum(f => f.LinesOfCode);

            var shownStatistics = folders
                .Where(f => f.LinesOfCode > 0)
                .OrderBy(f => f.Name)
                .ToList();

            var columns = folderColumns(totalLinesOfCode);

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
            StringBuilder builder, IList<string> cells,
            IList<int> rowWidths, IList<TextAlignMode> rowAlignments)
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

        private static List<(string, Func<Folder, string>, TextAlignMode)> folderColumns(int totalLinesOfCode)
            => new List<(string title, Func<Folder, object> value, TextAlignMode alignment)>
            {
                ("folder", f => f.Name, Left),
                ("loc", f => f.LinesOfCode.ToString("N"), Right),
                ("%", f => formatPercentage(f.LinesOfCode, totalLinesOfCode), Right),
            }.Select<(string, Func<Folder, object>, TextAlignMode), (string, Func<Folder, string>, TextAlignMode)>(
                t => (t.Item1, app => t.Item2(app).ToString(), t.Item3)).ToList();

        private static string formatPercentage(int count, int total)
        {
            var percentage = count * 100 / total;
            return percentage == 0 ? "<1%" : $"{percentage}%";
        }
    }
}
