using System;
using System.IO;
using System.Threading.Tasks;
using MobileStats.Bitrise;

namespace MobileStats
{
    internal class Program
    {
        private const string bitriseApiTokenVariable = "TOGGL_BITRISE_STATISTICS_API_TOKEN";
        private const string bitriseAppSlugsVariable = "TOGGL_BITRISE_APP_SLUGS";

        private const string outFile = "stats.txt";

        public static void Main(string[] args)
        {
            var bitriseApiToken = Environment.GetEnvironmentVariable(bitriseApiTokenVariable);
            var bitriseApps = Environment.GetEnvironmentVariable(bitriseAppSlugsVariable)
                .Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var bitriseReport = getBitriseStats(bitriseApiToken, bitriseApps).GetAwaiter().GetResult();

            var outPath = Path.Combine(Directory.GetCurrentDirectory(), outFile);

            Console.WriteLine($"Putting statistics into: ${outPath}");

            File.WriteAllText(outPath, bitriseReport);
        }

        private static async Task<string> getBitriseStats(string apiToken, string[] apps)
        {
            Console.WriteLine("Fetching bitrise builds...");
            var stats = await new Statistics(apiToken, apps).GetStatistics();

            Console.WriteLine("Preparing bitrise report...");
            var output = new Formatter().Format(stats);
            
            Console.WriteLine("Compiled Bitrise statistics:");
            Console.WriteLine(output);

            return output;
        }
    }
}
