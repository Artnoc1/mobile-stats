using System;

namespace MobileStats.AppCenter.Models
{
    class DatedCount
    {
        public DateTimeOffset Datetime { get; set; }
        public int Count { get; set; }
    }
}
