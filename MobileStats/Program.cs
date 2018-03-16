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

        private const string outDir = "output";
        private const string outStats = "stats.txt";
        private const string outImage = "bitrise-build-graph.png";

        public static void Main(string[] args)
        {
            var bitriseApiToken = Environment.GetEnvironmentVariable(bitriseApiTokenVariable);
            var bitriseApps = Environment.GetEnvironmentVariable(bitriseAppSlugsVariable)
                .Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var bitriseReport = getBitriseStats(bitriseApiToken, bitriseApps).GetAwaiter().GetResult();

            var outPath = Path.Combine(Directory.GetCurrentDirectory(), outDir);

            Directory.CreateDirectory(outPath);

            writeFile("statistics", outPath, outStats, path => File.WriteAllText(path, bitriseReport.text));
            writeFile("build graph", outPath, outImage, path => bitriseReport.buildGraph.Save(path, ImageFormat.Png));
        }

        private static void writeFile(string name, string basePath, string fileName, Action<string> writeToPath)
        {
            var path = Path.Combine(basePath, fileName);
            Console.WriteLine($"Putting {name} into: {path}");
            writeToPath(path);
        }

        private static async Task<(string text, Bitmap buildGraph)> getBitriseStats(string apiToken, string[] apps)
        {
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
