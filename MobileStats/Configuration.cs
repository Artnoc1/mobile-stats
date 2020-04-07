using System;

namespace MobileStats
{
    class Configuration : IConfiguration
    {
        public string BitriseApiToken { get; } = env("TOGGL_BITRISE_STATISTICS_API_TOKEN");
        public string BitriseAppSlugs { get; } = env("TOGGL_BITRISE_APP_SLUGS");
        
        public string AppCenterApiToken { get; } = env("TOGGL_APPCENTER_STATISTICS_API_TOKEN");
        public string AppCenterOwner { get; } = env("TOGGL_APPCENTER_STATISTICS_OWNER");
        public string AppCenterApps { get; } = env("TOGGL_APPCENTER_STATISTICS_APPS");
        
        public string AppFiguresUser { get; } = env("TOGGL_APP_FIGURES_STATISTICS_USER");
        public string AppFiguresPassword { get; } = env("TOGGL_APP_FIGURES_STATISTICS_PASSWORD");
        public string AppFiguresClientKey { get; } = env("TOGGL_APP_FIGURES_STATISTICS_CLIENT_KEY");
        public string AppFiguresProductIds { get; } = env("TOGGL_APP_FIGURES_STATISTICS_PRODUCT_IDS");

        private static string env(string key) => Environment.GetEnvironmentVariable(key);
    }
}