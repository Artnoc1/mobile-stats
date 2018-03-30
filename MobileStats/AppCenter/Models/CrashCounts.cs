using System.Collections.Generic;

namespace MobileStats.AppCenter.Models
{
    class CrashCounts
    {
        public int Count { get; set; }
        public List<DatedCount> Crashes { get; set; }
    }
}
