using System;
using System.Collections.Generic;
using System.Linq;
using MobileStats.Bitrise.Models;

namespace MobileStats.Bitrise
{
    class BuildCollectionStatistics
    {
        public int TotalCount { get; }
        public int StartedCount { get; }
        public int FinishedCount { get; }
        public int SuccessfulCount { get; }
        public int FailedCount { get; }
        public int AbortedCount { get; }

        public TimeSpan SuccessfulWorkingDurationAverage { get; }
        public TimeSpan SuccessfulWorkingDurationSTD { get; }

        public TimeSpan PendingDurationAverage { get; }
        public TimeSpan PendingDurationSTD { get; }
        public TimeSpan PendingDurationMedian { get; }
        public TimeSpan PendingDuration75Percent { get; }
        public TimeSpan PendingDuration95Percent { get; }

        public BuildCollectionStatistics(List<Build> builds)
        {
            TotalCount = builds.Count;

            if (TotalCount == 0)
                return;

            var startedBuilds = builds
                .Where(b => b.StartedOnWorkerAt.HasValue).ToList();
            var pendingDurations = startedBuilds
                .Select(b => b.PendingDuration.Value).ToList();

            var successfulBuilds = builds
                .Where(b => b.Status == BuildStatus.Success).ToList();
            var workingDurations = successfulBuilds
                .Select(b => b.WorkingDuration.Value).ToList();

            StartedCount = startedBuilds.Count;
            FinishedCount = builds.Count(b => b.FinishedAt.HasValue);
            FailedCount = builds.Count(b => b.Status == BuildStatus.Error);
            AbortedCount = builds.Count(b => b.Status == BuildStatus.Aborted);

            SuccessfulCount = successfulBuilds.Count;

            if (startedBuilds.Count == 0)
                return;

            PendingDurationAverage = pendingDurations.Average();
            PendingDurationSTD = pendingDurations.StandardDeviation();

            PendingDurationMedian = pendingDurations.PercentageValue(0.5);
            PendingDuration75Percent = pendingDurations.PercentageValue(0.75);
            PendingDuration95Percent = pendingDurations.PercentageValue(0.95);

            if (workingDurations.Count == 0)
                return;

            SuccessfulWorkingDurationAverage = workingDurations.Average();
            SuccessfulWorkingDurationSTD = workingDurations.StandardDeviation();

        }

    }
}
