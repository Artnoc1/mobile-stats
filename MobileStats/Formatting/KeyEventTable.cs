using System.Collections.Generic;
using System.Linq;
using MobileStats.AppCenter;
using MobileStats.AppCenter.Models;
using static System.Math;
using static MobileStats.TextAlignMode;

namespace MobileStats.Formatting
{
    static class KeyEventTable
    {
        public static string Format(List<AppStatistics> apps)
        {
            var events = Statistics.KeyEvents.Select(
                e => (
                    e.ShortName,
                    apps.ToDictionary(
                        a => a.App,
                        a => a.KeyEventCounts.First(e2 => e2.Event == e.Event).Count
                        )
                    )
            ).ToList();

            var iOS = "Toggl-iOS";
            var Android = "Toggl-Android";

            var table = TableFormatter.WithColumns<(string Name, Dictionary<string, EventCount> Count)>(
                ("event", e => e.Name, Left),
                ("iOS daily", e => format(daily(e.Count[iOS])), Left),
                ("And daily", e => format(daily(e.Count[Android])), Left),
                ("iOS weekly", e => format(weekly(e.Count[iOS])), Left),
                ("And weekly", e => format(weekly(e.Count[Android])), Left)
            );

            return table.Format(events);
        }

        private static string format(Percentage percentage)
        {
            var (samples, change) = (percentage.Samples, (percentage.Fraction - 1) * 100);

            var sampleString = samples switch
            {
                _ when samples < 100_000 => $"{samples}",
                _ when samples < 10_000_000 => $"{samples / 1000}K",
                _ when samples < 100_000_000 => $"{samples / 1000_000.0:0.0}M",
                _ => $"{samples / 1000_000}M",
            };

            var fractionString = change switch
            {
                _ when Abs(change) < 10 => $"{change:+0.0;-0.0}%",
                _ => $"{change:+0;-0}%"
            };

            return $"{sampleString.PadLeft(5)} {fractionString}";
        }

        private static Percentage daily(EventCount count)
        {
            var current = newToOld(count).First().Count;
            var previous = newToOld(count).Skip(7).First().Count;
            
            return Percentage.FromSamples(current, previous);
        }

        private static Percentage weekly(EventCount count)
        {
            var current = newToOld(count).Take(7).Sum(c => c.Count);
            var previous = newToOld(count).Skip(7).Take(7).Sum(c => c.Count);
            
            return Percentage.FromSamples(current, previous);
        }

        private static IEnumerable<DatedCount> newToOld(EventCount count)
        {
            return count.Count.OrderByDescending(c => c.Datetime).Skip(1);
        }
    }
}