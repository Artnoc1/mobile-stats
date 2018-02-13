using System;
using System.Collections.Generic;
using System.Linq;
using MobileStats.Bitrise.Models;

namespace MobileStats.Bitrise
{
    class AppBuildStatistics
    {
        public string Name { get; }
        public int TotalDays { get; }

        public BuildCollectionStatistics TotalStats { get; }
        public BuildCollectionStatistics LastDayStats { get; }

        public AppBuildStatistics(string name, int totalDays,
            DateTimeOffset now, List<Build> builds)
        {
            Name = name;
            TotalDays = totalDays;

            var oneDayAgo = now - TimeSpan.FromDays(1);

            var buildsInThePast = builds.Where(b => b.TriggeredAt < now).ToList();

            TotalStats = new BuildCollectionStatistics(buildsInThePast);
            LastDayStats = new BuildCollectionStatistics(
                buildsInThePast.Where(b => b.TriggeredAt > oneDayAgo).ToList());
        }
    }
}