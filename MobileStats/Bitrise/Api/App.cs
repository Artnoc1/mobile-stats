using System;
using System.Reactive.Linq;
using MobileStats.Bitrise.Models;

namespace MobileStats.Bitrise.Api
{
    class App : BaseApi
    {
        private readonly string appId;

        public App(string authToken, string appId)
            : base(authToken)
        {
            this.appId = appId;
        }
        
        public IObservable<Page<Build>> Builds(string page = null)
        {
            var path = page == null
                ? $"apps/{appId}/builds"
                : $"apps/{appId}/builds?next={page}";

            return RequestAndDeserialize<Page<Build>>(url(path));
        }

        public IObservable<Models.App> Info()
            => RequestAndDeserialize<Single<Models.App>>(url($"apps/{appId}"))
                .Select(single => single.Data);
    }
}
