using System.Linq;
using MobileStats.AppCenter.Models;

namespace MobileStats.AppCenter
{
    class AppVersionStatistics
    {
        public VersionCount Version { get; }
        public ActiveDeviceCounts ActiveDevices { get; }
        public CrashfreeDevicePercentages CrashfreePercentages { get; }
        public CrashCounts CrashCounts { get; }

        public int MostRecentWeeklyUsers { get; }
        public int MostRecentDailyUsers { get; }
        public double MostRecentCrashfreePercentage { get; }

        public AppVersionStatistics(VersionCount version,
            ActiveDeviceCounts activeDevices,
            CrashfreeDevicePercentages crashfreePercentages, CrashCounts crashCounts)
        {
            Version = version;
            ActiveDevices = activeDevices;
            CrashfreePercentages = crashfreePercentages;
            CrashCounts = crashCounts;

            MostRecentWeeklyUsers = activeDevices.Weekly
                .OrderByDescending(dc => dc.Datetime).First().Count;

            MostRecentDailyUsers = activeDevices.Daily
                .OrderByDescending(dc => dc.Datetime).Skip(1).First().Count;

            MostRecentCrashfreePercentage = CrashfreePercentages.DailyPercentages
                .OrderByDescending(dc => dc.Datetime).Skip(1).First().Percentage;
        }
    }
}
