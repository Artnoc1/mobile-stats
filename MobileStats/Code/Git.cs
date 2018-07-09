using System;
using System.Diagnostics;
using System.IO;

namespace MobileStats.Code
{
    public class Git
    {
        public void Clone(string repositoryPath, string localPath)
        {
            Console.WriteLine($"Cloning repository {repositoryPath} to {localPath}");

            if (Directory.Exists(localPath))
                Directory.Delete(localPath);

            Directory.CreateDirectory(localPath);

            var clone = Process.Start("git", $"clone {repositoryPath} {localPath}");

            clone.WaitForExit();
        }
    }
}
