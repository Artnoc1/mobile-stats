using System.Collections.Generic;

namespace MobileStats.Bitrise.Models
{
    class Page<T>
    {
        public List<T> Data { get; set; }
        public PageInfo Paging { get; set; }
    }
}
