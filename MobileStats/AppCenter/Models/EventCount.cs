using System.Collections.Generic;

namespace MobileStats.AppCenter.Models
{
    class EventCount
    {
        public string TotalCount { get; set; }
        public string PreviousTotalCount { get; set; }
        public List<DatedCount> Count { get; set; }
    }
}