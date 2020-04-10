using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MobileStats.Bitrise;
using MobileStats.Bitrise.Models;

namespace MobileStats.Formatting
{
    class BuildGraphPainter
    {
        private class CompareBuildsBySlug : IEqualityComparer<Build>
        {
            public static CompareBuildsBySlug Instance { get; } = new CompareBuildsBySlug();

            private CompareBuildsBySlug() { }
            
            public bool Equals(Build x, Build y)
                => x == y || (x != null && y != null && x.Slug == y.Slug);

            public int GetHashCode(Build build)
                => build.Slug.GetHashCode();
        }

        private const int imageWidth = 1440;
        private const int buildBarHeight = 6;
        private const int buildBarMargin = 6;
        private const int fontHeight = 10;
        
        private static readonly Dictionary<string, Color> appColors =
            new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
            {
                {"android", Color.ForestGreen},
                {"ios", Color.RoyalBlue},
                {"mobileapp", Color.Orange},
            };
        
        private static readonly Dictionary<string, Color> workflowColors =
            new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
            {
                {"Tests.Integration", Color.Purple},
                {"Tests.Integration.Notify", Color.Purple},
                {"Tests.Integration.FromBackend.Notify", Color.Purple},
                {"Tests.Sync.Notify", Color.Orchid},
                {"Failed Build", Color.Red},
            };

        private static readonly List<(Color Color, string Text)> colorLegend =
            appColors.Concat(workflowColors)
            .GroupBy(p => p.Value)
            .Select(g => (g.Key, string.Join(", ", g.Select(p => p.Key))))
            .ToList();

        public Bitmap Draw(List<WorkflowBuildStatistics> statistics)
        {
            Console.WriteLine("Drawing bitrise build graph..");
            var now = DateTimeOffset.UtcNow;
            var endTime = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
            var startTime = endTime - TimeSpan.FromDays(7);

            var allBuilds = statistics
                .Where(workflow => workflow.App != null)
                .SelectMany(workflow => workflow.Builds
                    .Distinct(CompareBuildsBySlug.Instance)
                    .Select(b => (workflow.App, Build: b))
                );
            var buildsLastSevenDays = allBuilds.Where(b =>
                b.Build.StartedOnWorkerAt.HasValue && b.Build.FinishedAt.HasValue
                && b.Build.TriggeredAt > startTime && b.Build.FinishedAt < endTime)
                .ToList();

            Console.WriteLine($"The graph has {buildsLastSevenDays.Count} builds");

            var concurrencies = divideBuildsOverConcurrencies(buildsLastSevenDays);
            Console.WriteLine($"The graph has {concurrencies.Count} concurrencies");

            return drawGraph(concurrencies, startTime, endTime);
        }

        private List<List<(App App, Build Build)>> divideBuildsOverConcurrencies(IEnumerable<(App App, Build Build)> builds)
        {
            var concurrencies = new List<List<(App App, Build Build)>>();

            foreach (var (app, build) in builds.OrderBy(b => b.Build.StartedOnWorkerAt))
            {
                var concurrencyIndex = concurrencies.FindIndex(c => c.Last().Build.FinishedAt <= build.StartedOnWorkerAt);

                if (concurrencyIndex == -1)
                {
                    concurrencies.Add(new List<(App, Build)> { (app, build) });
                }
                else
                {
                    concurrencies[concurrencyIndex].Add((app, build));
                }
            }

            return concurrencies;
        }

        private Bitmap drawGraph(List<List<(App App, Build Build)>> concurrencies,
            DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var graphDuration = endTime - startTime;
            var concurrencySpacing = buildBarHeight + buildBarMargin;
            var graphHeight = (concurrencies.Count + 1) * concurrencySpacing;
            var legendHeight = (int)(colorLegend.Count * fontHeight * 1.5) + fontHeight;
            var imageHeight = graphHeight + legendHeight + fontHeight * 3;

            var bitmap = new Bitmap(imageWidth, imageHeight);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                // vertical grid
                var dayLinePen = new Pen(Color.LightGray);
                var hourLinePen = new Pen(Color.FromArgb(240, 240, 240));
                var font = new Font(FontFamily.GenericSansSerif, fontHeight);
                var fontBrush = new SolidBrush(Color.DimGray);
                var dayWidth = imageWidth / graphDuration.TotalDays;
                var hourWidth = dayWidth / 24;
                var dayFormat = new StringFormat { Alignment = StringAlignment.Center };
                foreach (var day in Enumerable.Range(0, (int)graphDuration.TotalDays))
                {
                    var x = (int)(day * dayWidth);
                    if (day > 0)
                        graphics.DrawLine(dayLinePen, x, 0, x, imageHeight - legendHeight);
                    graphics.DrawString($"{startTime + TimeSpan.FromDays(day):ddd yyyy-MM-dd}",
                        font, fontBrush, x + (int)(dayWidth * 0.5), (int)(graphHeight + fontHeight * 1.5), dayFormat);

                    foreach (var hour in Enumerable.Range(1, 23))
                    {
                        x = (int)(day * dayWidth + hour * hourWidth);
                        graphics.DrawLine(hourLinePen, x, 0, x, graphHeight);
                        
                        if (hour % 4 == 0)
                            graphics.DrawString($"{hour}h", font, fontBrush, x, graphHeight, dayFormat);
                    }
                }
                
                // horizontal grid
                var concurrencyLinePen = new Pen(Color.DarkGray);
                foreach (var concurrency in Enumerable.Range(1, concurrencies.Count))
                {
                    var y = graphHeight - concurrency * concurrencySpacing;
                    graphics.DrawLine(concurrencyLinePen, 0, y, imageWidth, y);
                    var textY = y - concurrencySpacing - buildBarHeight / 2;
                    graphics.DrawString(concurrency.ToString(), font, fontBrush, 0, textY);
                }
                
                // load background
                var allBuilds = concurrencies.SelectMany(x => x)
                    .Select(b => (b.App, Start: b.Build.StartedOnWorkerAt.Value, End: b.Build.FinishedAt.Value));
                var averageDuration = allBuilds.Average(b => b.End - b.Start);
                var durationRadius = TimeSpan.FromMinutes(30);
                var graphScalar = concurrencySpacing * 0.66 * durationRadius.TotalDays / averageDuration.TotalDays;
                foreach (var x in Enumerable.Range(0, imageWidth))
                {
                    var time = startTime + TimeSpan.FromDays(x * graphDuration.TotalDays / imageWidth);
                    var intervalStart = time - durationRadius;

                    var overlap = allBuilds
                        .Select(b =>
                        (
                            b.App,
                            Overlap: Math.Min(1, (b.End - intervalStart).TotalDays / (durationRadius.TotalDays * 2))
                                     - Math.Max(0, (b.Start - intervalStart).TotalDays / (durationRadius.TotalDays * 2))
                        ))
                        .Where(b => b.Overlap > 0)
                        .GroupBy(b => buildColor(b.App, null))
                        .Select(g => (Color: g.Key, Amount: g.Sum(b => b.Overlap)))
                        .OrderBy(o => o.Color.GetHue());

                    var y = graphHeight;
                    foreach (var (color, amount) in overlap)
                    {
                        var height = (int)(amount * graphScalar);
                        graphics.DrawRectangle(new Pen(Color.FromArgb(40, color)),
                            x, y - height, 1, height
                        );
                        y -= height;
                    }
                }

                // base line
                graphics.DrawLine(new Pen(Color.DimGray), 0, graphHeight, imageWidth, graphHeight);

                // builds
                var failedBrush = new SolidBrush(Color.Red);
                
                foreach (var (builds, concurrency) in concurrencies.Select((bs, c) => (bs, c)))
                {
                    var y = graphHeight - (concurrency + 1) * concurrencySpacing - buildBarHeight / 2;

                    foreach (var (app, build) in builds)
                    {
                        var buildBrush = new SolidBrush(buildColor(app, build));
                        
                        var timeSinceStartTime = build.StartedOnWorkerAt.Value - startTime;
                        var x = (int)(timeSinceStartTime.TotalDays / graphDuration.TotalDays * imageWidth);
                        var w = (int)Math.Ceiling(build.WorkingDuration.Value.TotalDays / graphDuration.TotalDays * imageWidth);
                        
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

        private Color buildColor(App app, Build build)
        {
            return workflowColors.TryGetValue(build?.TriggeredWorkflow ?? "", out var color)
                || appColors.TryGetValue(app.Title ?? "", out color)
                    ? color : Color.DimGray;
        }
    }
}
