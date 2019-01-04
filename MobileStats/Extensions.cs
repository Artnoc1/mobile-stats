using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MobileStats
{
    static class Extensions
    {
        public static TimeSpan Average<T>(this IEnumerable<T> source, Func<T, TimeSpan> selector)
            => source.Select(selector).Average();

        public static TimeSpan Average(this IEnumerable<TimeSpan> source)
            => TimeSpan.FromSeconds(source.Average(t => t.TotalSeconds));

        public static TimeSpan StandardDeviation(this IEnumerable<TimeSpan> source)
        {
            var seconds = source.Select(t => t.TotalSeconds).ToList();
            var averageSeconds = seconds.Average();
            var varianceSeconds = seconds.Average(s => (s - averageSeconds).Squared());
            return TimeSpan.FromSeconds(Math.Sqrt(varianceSeconds));
        }

        public static TimeSpan PercentageValue(this IEnumerable<TimeSpan> source, double percentage)
        {
            var sorted = source.OrderBy(t => t.Ticks).ToList();
            var index = (int) (sorted.Count * percentage);
            return sorted[index];
        }

        public static double Squared(this double value) => value * value;

        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
        
        public static IEnumerable<T> OrderAlphanumericallyDescendingBy<T>(this IEnumerable<T> source, Func<T, string> keySelector)
        {
            // very crude way to help sort by version number and similar strings containing numbers
            return source.OrderByDescending(item => Regex
                .Replace(keySelector(item),
                    @"\d+",
                    m => m.Value.PadLeft(50, '0')
                ));
        }
    }
}
