using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MobileStats.AppCenter;
using MobileStats.Bitrise;
using MobileStats.Code;
using Formatter = MobileStats.Bitrise.Formatter;
using Statistics = MobileStats.Bitrise.Statistics;

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
            var codeReport = getCodeStats().GetAwaiter().GetResult();

            var outPath = Path.Combine(Directory.GetCurrentDirectory(), outDir);

            Directory.CreateDirectory(outPath);

            writeFile("statistics", outPath, outStats, path => File.WriteAllText(path,
                $"{appCenterReport}\n{bitriseReport.text}\n{codeReport}"));
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
            var apps = Environment.GetEnvironmentVariable(appCenterAppsVariable)
                .Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var formatter = new AppCenter.Formatter();

            var stats = new List<AppStatistics>();

            foreach (var app in apps)
            {
                Console.WriteLine($"Fetching app center statistics for {app}...");
                stats.Add(await new AppCenter.Statistics(apiToken, owner, app).GetStatistics());
            }
            Console.WriteLine("Preparing app center report...");

            var summaryTable = formatter.FormatSummaryTable(stats);

            var breakdownTables = "Yesterday's breakdowns:\n"
                + string.Join("", stats.Select(s => $"{formatter.FormatNameWithEmoji(s)}\n{formatter.Format(s.VersionStatistics)}"));

            var output = summaryTable + breakdownTables;

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


        private static async Task<string> getCodeStats()
        {
            const string repo = "git@github.com:toggl/mobileapp.git";
            var tempPath = Path.GetTempPath() + Guid.NewGuid();
            var repoPath = tempPath + "mobileapp";
            new Git().Clone(repo, repoPath);

            Console.WriteLine("Gathering data...");
            var folders = new Code.Statistics(repoPath).CompileFolderStatistics();

            Console.WriteLine("Cleaning up cloned repository...");

            if(Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);

            Console.WriteLine("Preparing code report...");
            var formatter = new Code.Formatter();
            var linesOfCode = formatter.FormatLinesOfCode(folders);
            var kpis = formatter.FormatKPIs(folders);

            var output =
                $"*Code statistics*\n" +
                $"{linesOfCode}\n" +
                $"{kpis}";

            Console.WriteLine("Compiled code report:");
            Console.WriteLine(output);

            return output;
        }

    }
}
