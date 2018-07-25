using System.Collections.Generic;

namespace MobileStats.AppFigures.Models
{
    class Rating
    {
        public long Product { get; set; }
        public List<int> Stars { get; set; }
    }
}
