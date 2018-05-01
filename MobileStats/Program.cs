using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using MobileStats.Bitrise;

namespace MobileStats
{
    internal class Program
    {
        private const string bitriseApiTokenVariable = "TOGGL_BITRISE_STATISTICS_API_TOKEN";
        private const string bitriseAppSlugsVariable = "TOGGL_BITRISE_APP_SLUGS";

        private const string appCenterApiTokenVariable = "TOGGL_APPCENTER_STATISTICS_API_TOKEN";
        private const string appCenterOwnerVariable = "TOGGL_APPCENTER_STATISTICS_OWNER";
        private const string appCenterAppsVariable = "TOGGL_APPCENTER_STATISTICS_APPS";

        private const string outDir = "output";
        private const string outStats = "stats.txt";
        private const string outImage = "bitrise-build-graph.png";

        public static void Main(string[] args)
        {
            var appCenterReport = getAppCenterStats().GetAwaiter().GetResult();
            var bitriseReport = getBitriseStats().GetAwaiter().GetResult();

            var outPath = Path.Combine(Directory.GetCurrentDirectory(), outDir);

            Directory.CreateDirectory(outPath);

            writeFile("statistics", outPath, outStats, path => File.WriteAllText(path,
                $"{appCenterReport}\n{bitriseReport.text}"));
            writeFile("build graph", outPath, outImage, path => bitriseReport.buildGraph.Save(path, ImageFormat.Png));
        }

        private static void writeFile(string name, string basePath, string fileName, Action<string> writeToPath)
        {
            var path = Path.Combine(basePath, fileName);
            Console.WriteLine($"Putting {name} into: {path}");
            writeToPath(path);
        }

        private static async Task<string> getAppCenterStats()
        {
            var apiToken = Environment.GetEnvironmentVariable(appCenterApiTokenVariable);
            var owner = Environment.GetEnvironmentVariable(appCenterOwnerVariable);
            var app = Environment.GetEnvironmentVariable(appCenterAppsVariable);


            Console.WriteLine("Fetching app center statistics...");
            var stats = await new AppCenter.Statistics(apiToken, owner, app).GetStatistics();

            Console.WriteLine("Preparing app center report...");
            var output = $"Daneel has *{stats.Totals.MostRecentWeeklyUsers} weekly users*"
                + $" and is *stable for {stats.Totals.MostRecentCrashfreePercentage * 100:0.00}%* of them.\n"
                + "Yesterday's breakdown:\n"
                + new AppCenter.Formatter().Format(stats.VersionStatistics);

            Console.WriteLine("Compiled app center report:");
            Console.WriteLine(output);

            return output;
        }

        private static async Task<(string text, Bitmap buildGraph)> getBitriseStats()
        {
            var apiToken = Environment.GetEnvironmentVariable(bitriseApiTokenVariable);
            var apps = Environment.GetEnvironmentVariable(bitriseAppSlugsVariable)
                .Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            Console.WriteLine("Fetching bitrise builds...");
            var stats = await new Statistics(apiToken, apps).GetStatistics();

            Console.WriteLine("Preparing bitrise report...");
            var output = new Formatter().Format(stats);

            Console.WriteLine("Compiled Bitrise statistics:");
            Console.WriteLine(output);

            var buildGraph = new BuildGraphPainter().Draw(stats);

            return (output, buildGraph);
        }
    }
}
