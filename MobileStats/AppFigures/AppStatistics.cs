using System.Linq;
using MobileStats.AppFigures.Models;

namespace MobileStats.AppFigures
{
    class AppStatistics
    {
        public string AppId { get; }
        public long Id { get; }
        public double Rating { get; }

        public AppStatistics(Product product, Rating rating)
        {
            AppId = product.BundleIdentifier;
            Id = product.Id;

            var stars = rating.Stars;
            var totalRatings = Enumerable.Range(0, 5).Sum(i => stars[i]);
            var weightedTotal = Enumerable.Range(0, 5).Sum(i => stars[i] * (i + 1));
            Rating = weightedTotal / (double)totalRatings;
        }
    }
}
