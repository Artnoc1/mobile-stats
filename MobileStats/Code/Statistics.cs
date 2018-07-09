using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MobileStats.Code
{
    public class Statistics
    {
        private readonly string path;

        public Statistics(string path)
        {
            this.path = path;
        }

        public List<Folder> CompileFolderStatistics()
        {
            var directories = Directory.GetDirectories(path, "Toggl.*", SearchOption.TopDirectoryOnly);

            return directories
                .Select(getFolderStatistics)
                .ToList();
        }

        private Folder getFolderStatistics(string path)
        {
            var csFiles = Directory
                .GetFiles(path, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith(".designer.cs"));

            return new Folder
            {
                Name = new FileInfo(path).Name.Replace("Toggl.", ""),
                LinesOfCode = csFiles.Sum(linesInFile)
            };
        }

        private int linesInFile(string filename)
        {
            return File.ReadAllLines(filename).Count(l => !string.IsNullOrWhiteSpace(l));
        }
    }
}
