using MobileStats.AppFigures.Models;

namespace MobileStats.AppFigures
{
    class AppStatistics
    {
        public string AppId { get; }
        public long Id { get; }
        public double Rating { get; }

        public AppStatistics(Product product, RatingsReport rating)
        {
            AppId = product.BundleIdentifier;
            Id = product.Id;
            
            Rating = rating.NewAverage;
        }
    }
}
