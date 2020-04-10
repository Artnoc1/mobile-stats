using System;
using System.Linq;
using MobileStats.AppCenter.Models;
using Newtonsoft.Json;

namespace MobileStats.AppCenter.Api
{
    class App
    {
        public delegate IObservable<T> StartEndVersionFilteredEndpoint<out T>(
            DateTimeOffset startTime, DateTimeOffset? endTime = null, string[] versions = null);

        private readonly BaseApi api;
        private readonly string owner;
        private readonly string appName;

        public App(BaseApi api, string owner, string appName)
        {
            this.api = api;
            this.owner = owner;
            this.appName = appName;
        }
        
        public Event Event(string eventName) => new Event(this, eventName);

        public IObservable<VersionCollection> Versions(DateTimeOffset startTime)
            => api.RequestAndDeserialize<VersionCollection>(
                AppUrl("analytics/versions",
                    ("start", formatDateTime(startTime))
                ));

        public StartEndVersionFilteredEndpoint<ActiveDeviceCounts> ActiveDeviceCounts =>
            FilteredEndpoint<ActiveDeviceCounts>("analytics/active_device_counts");

        public StartEndVersionFilteredEndpoint<CrashfreeDevicePercentages> CrashfreeDevicePercentages =>
            FilteredEndpoint<CrashfreeDevicePercentages>("errors/errorfreeDevicePercentages", api.CamelCase, ("errorType", "unhandlederror"));

        public StartEndVersionFilteredEndpoint<CrashCounts> CrashCounts =>
            FilteredEndpoint<CrashCounts>("errors/errorCountsPerDay", null, ("errorType", "unhandlederror"));

        public StartEndVersionFilteredEndpoint<T> FilteredEndpoint<T>(
            string path,
            JsonSerializerSettings serialiserSettings = null,
            params (string name, string value)[] parameters)
            => (startTime, endTime, versions) => api.RequestAndDeserialize<T>(
                AppUrl(path,
                    new[] {
                        ("start", formatDateTime(startTime)),
                        ("end", formatDateTime(endTime)),
                        ("versions", versionArrayToParameter(versions))
                    }.Concat(parameters).ToArray()
                ), serialiserSettings);

        private string formatDateTime(DateTimeOffset? time)
            => time?.ToString("O");

        private string versionArrayToParameter(string[] versions)
            => versions == null || versions.Length == 0
                ? null : string.Join("|", versions);

        public string AppUrl(string path, params (string name, string value)[] parameters)
            => api.Url($"apps/{owner}/{appName}/{path}", parameters);
    }
}
