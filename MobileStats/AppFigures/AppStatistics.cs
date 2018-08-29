using MobileStats.AppFigures.Models;

namespace MobileStats.AppFigures
{
    class AppStatistics
    {
        public string AppId { get; }
        public long Id { get; }
        public double RatingAverage { get; }
        public double RecentRatingAverage { get; }

        public AppStatistics(Product product, RatingsReport rating)
        {
            AppId = product.BundleIdentifier;
            Id = product.Id;
            
            RatingAverage = rating.Average;
            RecentRatingAverage = rating.NewAverage;
        }
    }
}
