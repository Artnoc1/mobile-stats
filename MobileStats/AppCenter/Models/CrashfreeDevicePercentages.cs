using System.Collections.Generic;

namespace MobileStats.AppCenter.Models
{
    class CrashfreeDevicePercentages
    {
        public double AveragePercentage { get; set; }
        public List<DatedPercentage> DailyPercentages { get; set; }
    }
}
