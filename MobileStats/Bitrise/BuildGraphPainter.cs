using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MobileStats.Bitrise.Models;

namespace MobileStats.Bitrise
{
    class BuildGraphPainter
    {
        private class CompareBuildsBySlug : IEqualityComparer<Build>
        {
            public bool Equals(Build x, Build y)
                => x == y || (x != null && y != null && x.Slug == y.Slug);

            public int GetHashCode(Build build)
                => build.Slug.GetHashCode();
        }

        private const int imageWidth = 1440;
        private const int buildBarHeight = 6;
        private const int buildBarMargin = 6;
        private const int fontHeight = 10;
        
        private static readonly Dictionary<string, Color> workflowColors =
            new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
            {
                {"Tests.AppBuilds.Android", Color.ForestGreen},
                {"Tests.AppBuilds.iOS", Color.SkyBlue},
                {"Tests.Unit.Split", Color.DarkCyan},
                {"Tests.Integration", Color.Purple},
                {"Tests.Integration.Notify", Color.Purple},
                {"Tests.Integration.FromBackend.Notify", Color.Purple},
                {"Tests.Sync.Notify", Color.Orchid},
                {"Failed Build", Color.Red},
            };

        private static readonly List<(Color Color, string Text)> colorLegend = workflowColors
            .GroupBy(p => p.Value)
            .Select(g => (g.Key, string.Join(", ", g.Select(p => p.Key))))
            .ToList();

        public Bitmap Draw(List<WorkflowBuildStatistics> statistics)
        {
            Console.WriteLine("Drawing bitrise build graph..");
            var now = DateTimeOffset.UtcNow;
            var endTime = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
            var startTime = endTime - TimeSpan.FromHours(24);

            var allBuilds = statistics.SelectMany(workflow => workflow.Builds).Distinct(new CompareBuildsBySlug());
            var buildsYesterday = allBuilds.Where(b =>
                b.StartedOnWorkerAt.HasValue && b.FinishedAt.HasValue
                && b.TriggeredAt > startTime && b.FinishedAt < endTime)
                .ToList();

            Console.WriteLine($"The graph has {buildsYesterday.Count} builds");

            var concurrencies = divideBuildsOverConcurrencies(buildsYesterday);
            Console.WriteLine($"The graph has {concurrencies.Count} concurrencies");

            return drawGraph(concurrencies, startTime);
        }

        private List<List<Build>> divideBuildsOverConcurrencies(IEnumerable<Build> buildsYesterday)
        {
            var concurrencies = new List<List<Build>>();

            foreach (var build in buildsYesterday.OrderBy(b => b.StartedOnWorkerAt))
            {
                var concurrencyIndex = concurrencies.FindIndex(c => c.Last().FinishedAt <= build.StartedOnWorkerAt);

                if (concurrencyIndex == -1)
                {
                    concurrencies.Add(new List<Build> { build });
                }
                else
                {
                    concurrencies[concurrencyIndex].Add(build);
                }
            }

            return concurrencies;
        }

        private Bitmap drawGraph(List<List<Build>> concurrencies, DateTimeOffset startTime)
        {
            var concurrencySpacing = buildBarHeight + buildBarMargin;
            var graphHeight = (concurrencies.Count + 1) * concurrencySpacing;
            var legendHeight = (int)(colorLegend.Count * fontHeight * 1.5) + fontHeight;
            var imageHeight = graphHeight + legendHeight + fontHeight * 3;

            var bitmap = new Bitmap(imageWidth, imageHeight);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);

                // grid
                var hourLinePen = new Pen(Color.LightGray);
                var font = new Font(FontFamily.GenericSansSerif, fontHeight);
                var fontBrush = new SolidBrush(Color.DimGray);
                foreach (var hour in Enumerable.Range(1, 23))
                {
                    var x = hour * imageWidth / 24;
                    graphics.DrawLine(hourLinePen, x, 0, x, imageHeight - legendHeight);
                    graphics.DrawString($"{hour}h", font, fontBrush, x, graphHeight);
                }
                var concurrencyLinePen = new Pen(Color.Gray);
                foreach (var concurrency in Enumerable.Range(1, concurrencies.Count))
                {
                    var y = graphHeight - concurrency * concurrencySpacing;
                    graphics.DrawLine(concurrencyLinePen, 0, y, imageWidth, y);
                    var textY = y - concurrencySpacing - buildBarHeight / 2;
                    graphics.DrawString(concurrency.ToString(), font, fontBrush, 0, textY);
                }

                // base line
                graphics.DrawLine(new Pen(Color.DimGray), 0, graphHeight, imageWidth, graphHeight);

                // builds
                var failedBrush = new SolidBrush(Color.Red);
                
                foreach (var (builds, concurrency) in concurrencies.Select((bs, c) => (bs, c)))
                {
                    var y = graphHeight - (concurrency + 1) * concurrencySpacing - buildBarHeight / 2;

                    foreach (var build in builds)
                    {
                        var buildBrush = new SolidBrush(buildColor(build));
                        
                        var timeSinceStartTime = build.StartedOnWorkerAt.Value - startTime;
                        var x = (int)(timeSinceStartTime.TotalDays * imageWidth);
                        var w = (int)(build.WorkingDuration.Value.TotalDays * imageWidth);

                        w = Math.Max(w, 1);

                        graphics.FillRectangle(buildBrush, x, y, w, buildBarHeight);

                        if (build.Status != BuildStatus.Success)
                        {
                            graphics.FillRectangle(failedBrush, x + w - 1, y, 1, buildBarHeight);
                        }
                    }
                }
                
                // legend
                foreach (var (legend, i) in colorLegend.Select((l, i) => (l, i)))
                {
                    var squareBrush = new SolidBrush(legend.Color);

                    var x = 10;
                    var y = (int)(graphHeight + i * 1.5 * fontHeight) + fontHeight * 3;
                    var w = fontHeight - 2;
                    
                    graphics.FillRectangle(squareBrush, x + 1, y + 1 + w / 2, w, w);
                    graphics.DrawString(legend.Text, font, fontBrush, x + w + 4, y);
                }
            }

            return bitmap;
        }

        private Color buildColor(Build build)
        {
            return workflowColors.TryGetValue(build.TriggeredWorkflow, out var color) ? color : Color.DimGray;
        }
    }
}
