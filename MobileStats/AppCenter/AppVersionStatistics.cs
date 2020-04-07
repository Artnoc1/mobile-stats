using System.Collections.Generic;
using System.Linq;
using MobileStats.AppCenter.Models;
using MobileStats.Formatting;

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
        public int MostRecentMonthlyUsers { get; }
        public double MostRecentCrashfreePercentage { get; }
        
        public Percentage MostRecentDailyUsersChange { get; }
        public Percentage MostRecentDailyUsersChangeWeekly { get; }
        public Percentage MostRecentWeeklyUsersChange { get; }
        public Percentage MostRecentMonthlyUsersChange { get; }

        public AppVersionStatistics(VersionCount version,
            ActiveDeviceCounts activeDevices,
            CrashfreeDevicePercentages crashfreePercentages, CrashCounts crashCounts)
        {
            Version = version;
            ActiveDevices = activeDevices;
            CrashfreePercentages = crashfreePercentages;
            CrashCounts = crashCounts;

            MostRecentMonthlyUsers = mostRecent(activeDevices.Monthly);
            
            MostRecentWeeklyUsers = mostRecent(activeDevices.Weekly);

            MostRecentDailyUsers = mostRecent(activeDevices.Daily);

            MostRecentCrashfreePercentage = CrashfreePercentages.DailyPercentages
                .OrderByDescending(dc => dc.Datetime).Skip(1).First().Percentage;

            var secondMostRecentMonthlyUsers = mostRecent(activeDevices.Monthly, 30);
            var secondMostRecentWeeklyUsers = mostRecent(activeDevices.Weekly, 7);
            var secondMostRecentDailyUsers = mostRecent(activeDevices.Daily, 1);
            var mostRecentDailyUsersLastWeek = mostRecent(activeDevices.Daily, 7);
            
            MostRecentMonthlyUsersChange = Percentage.FromSamples(MostRecentMonthlyUsers, secondMostRecentMonthlyUsers);
            MostRecentWeeklyUsersChange = Percentage.FromSamples(MostRecentWeeklyUsers, secondMostRecentWeeklyUsers);
            MostRecentDailyUsersChange = Percentage.FromSamples(MostRecentDailyUsers, secondMostRecentDailyUsers);
            MostRecentDailyUsersChangeWeekly = Percentage.FromSamples(MostRecentDailyUsers, mostRecentDailyUsersLastWeek);
        }

        private static int mostRecent(List<DatedCount> counts, int addDays = 0)
            => counts.OrderByDescending(dc => dc.Datetime).Skip(1 + addDays).First().Count;
    }
}
