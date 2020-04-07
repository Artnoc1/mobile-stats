using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using MobileStats.Formatting;

namespace MobileStats
{
    internal class Program
    {
        private static readonly IConfiguration config = new Configuration();
        
        private const string outDir = "output";
        private const string outStats = "stats.txt";
        private const string outImage = "bitrise-build-graph.png";

        public static async Task Main(string[] args)
        {
            var appFiguresData = await new AppFigures.Statistics(config).FetchStats();
            var appCenterData = await new AppCenter.StatisticsGatherer(config).FetchAll();
            var bitriseData = await new Bitrise.Statistics(config).GetStatistics();

            var report =
$@"Vitals
{
    VitalsTable.Format(appCenterData, appFiguresData)
        .logReport("vitals table")
}
Active users
{
    ActiveUsersTable.Format(appCenterData)
        .logReport("active user table")
}
Yesterday's breakdowns
{
    DailyBreakdownTable.Format(appCenterData)
        .logReport("yesterday's breakdowns")
}
Bitrise build report
{
    BuildStatisticsTable.Format(bitriseData)
        .logReport("bitrise build report")
}
";

            var outPath = getOutPath();

            writeFile("statistics", outPath, outStats, path => File.WriteAllText(path, report));

            var buildGraph = new Bitrise.BuildGraphPainter().Draw(bitriseData);
            writeFile("build graph", outPath, outImage, path => buildGraph.Save(path, ImageFormat.Png));
        }

        private static string getOutPath()
        {
            var outPath = Path.Combine(Directory.GetCurrentDirectory(), outDir);
            Directory.CreateDirectory(outPath);
            return outPath;
        }

        private static void writeFile(string name, string basePath, string fileName, Action<string> writeToPath)
        {
            var path = Path.Combine(basePath, fileName);
            Console.WriteLine($"Putting {name} into: {path}");
            writeToPath(path);
        }
    }
}
