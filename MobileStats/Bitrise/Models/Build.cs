using System;

namespace MobileStats.Bitrise.Models
{
    class Build
    {
        public string Branch { get; set; }
        public BuildStatus Status { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public DateTimeOffset? StartedOnWorkerAt { get; set; }
        public DateTimeOffset TriggeredAt { get; set; }
        public string TriggeredWorkflow { get; set; }

        public TimeSpan? PendingDuration => StartedOnWorkerAt == null
            ? null
            : StartedOnWorkerAt - TriggeredAt;

        public TimeSpan? WorkingDuration => FinishedAt == null
            ? null
            : FinishedAt - StartedOnWorkerAt;
    }
}
