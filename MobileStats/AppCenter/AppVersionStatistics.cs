using System.Linq;
using MobileStats.AppCenter.Models;

namespace MobileStats.AppCenter
{
    class AppVersionStatistics
    {
        public ActiveDeviceCounts ActiveDevices { get; }
        public CrashfreeDevicePercentages CrashfreePercentages { get; }
        public CrashCounts CrashCounts { get; }

        public int MostRecentWeeklyUsers { get; }
        public double MostRecentCrashfreePercentage { get; }

        public AppVersionStatistics(ActiveDeviceCounts activeDevices,
            CrashfreeDevicePercentages crashfreePercentages, CrashCounts crashCounts)
        {
            ActiveDevices = activeDevices;
            CrashfreePercentages = crashfreePercentages;
            CrashCounts = crashCounts;

            MostRecentWeeklyUsers = activeDevices.Weekly
                .OrderByDescending(dc => dc.Datetime).First().Count;

            MostRecentCrashfreePercentage = CrashfreePercentages.DailyPercentages
                .OrderByDescending(dc => dc.Datetime).First().Percentage;
        }
    }
}
