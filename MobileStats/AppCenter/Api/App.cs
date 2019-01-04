using System;
using System.Linq;
using MobileStats.AppCenter.Models;
using Newtonsoft.Json;

namespace MobileStats.AppCenter.Api
{
    class App : BaseApi
    {
        public delegate IObservable<T> StartEndVersionFilteredEndpoint<out T>(
            DateTimeOffset startTime, DateTimeOffset? endTime = null, string[] versions = null);

        private readonly string owner;
        private readonly string appName;

        public App(string authToken, string owner, string appName)
            : base(authToken)
        {
            this.owner = owner;
            this.appName = appName;
        }

        public IObservable<VersionCollection> Versions(DateTimeOffset startTime)
            => RequestAndDeserialize<VersionCollection>(
                appUrl("analytics/versions",
                    ("start", formatDateTime(startTime))
                ));

        public StartEndVersionFilteredEndpoint<ActiveDeviceCounts> ActiveDeviceCounts =>
            startEndVersionFilteredEndpoint<ActiveDeviceCounts>("analytics/active_device_counts");

        public StartEndVersionFilteredEndpoint<CrashfreeDevicePercentages> CrashfreeDevicePercentages =>
            startEndVersionFilteredEndpoint<CrashfreeDevicePercentages>("errors/errorfreeDevicePercentages", CamelCase, ("errorType", "unhandlederror"));

        public StartEndVersionFilteredEndpoint<CrashCounts> CrashCounts =>
            startEndVersionFilteredEndpoint<CrashCounts>("errors/errorCountsPerDay", null, ("errorType", "unhandlederror"));

        private StartEndVersionFilteredEndpoint<T> startEndVersionFilteredEndpoint<T>(
            string path,
            JsonSerializerSettings serialiserSettings = null,
            params (string name, string value)[] parameters)
            => (startTime, endTime, versions) => RequestAndDeserialize<T>(
                appUrl(path,
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

        private string appUrl(string path, params (string name, string value)[] parameters)
            => url($"apps/{owner}/{appName}/{path}", parameters);
    }
}
