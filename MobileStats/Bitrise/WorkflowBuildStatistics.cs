﻿using System;
using System.Collections.Generic;
using System.Linq;
using MobileStats.Bitrise.Models;
using App = MobileStats.Bitrise.Api.App;

namespace MobileStats.Bitrise
{
    class WorkflowBuildStatistics
    {
        public Models.App App { get; }
        public string Name { get; }
        public int TotalDays { get; }

        public List<Build> Builds { get; }

        public BuildCollectionStatistics TotalStats { get; }
        public BuildCollectionStatistics LastDayStats { get; }

        public WorkflowBuildStatistics(Models.App app, string name, int totalDays,
            DateTimeOffset now, List<Build> builds)
        {
            App = app;
            Name = name;
            TotalDays = totalDays;

            Builds = builds;

            var oneDayAgo = now - TimeSpan.FromDays(1);

            var buildsInThePast = builds.Where(b => b.TriggeredAt < now).ToList();

            TotalStats = new BuildCollectionStatistics(buildsInThePast);
            LastDayStats = new BuildCollectionStatistics(
                buildsInThePast.Where(b => b.TriggeredAt > oneDayAgo).ToList());
        }
    }
}
