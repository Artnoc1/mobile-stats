using MobileStats.AppCenter.Models;

namespace MobileStats.AppCenter.Api
{
    class Event
    {
        private readonly App appApi;
        private readonly string eventName;

        public Event(App app, string eventName)
        {
            appApi = app;
            this.eventName = eventName;
        }

        public App.StartEndVersionFilteredEndpoint<EventCount> EventCount =>
            appApi.FilteredEndpoint<EventCount>($"analytics/app_events/{eventName}/event_count");
    }
}