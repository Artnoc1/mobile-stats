using System.Collections.Generic;

namespace MobileStats.AppCenter.Models
{
    class ActiveDeviceCounts
    {
        public List<DatedCount> Daily { get; set; }
        public List<DatedCount> Weekly { get; set; }
        public List<DatedCount> Monthly { get; set; }
    }
}
