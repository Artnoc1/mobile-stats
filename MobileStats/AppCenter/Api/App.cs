﻿using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using MobileStats.AppCenter.Models;

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

        public IObservable<List<VersionInfo>> Versions()
            => RequestAndDeserialize<List<VersionInfo>>(appUrl("versions"));

        public StartEndVersionFilteredEndpoint<ActiveDeviceCounts> ActiveDeviceCounts =>
            startEndVersionFilteredEndpoint<ActiveDeviceCounts>("analytics/active_device_counts");

        public StartEndVersionFilteredEndpoint<CrashfreeDevicePercentages> CrashfreeDevicePercentages =>
            startEndVersionFilteredEndpoint<CrashfreeDevicePercentages>("analytics/crashfree_device_percentages");

        public StartEndVersionFilteredEndpoint<CrashCounts> CrashCounts =>
            startEndVersionFilteredEndpoint<CrashCounts>("analytics/crash_counts");

        private StartEndVersionFilteredEndpoint<T> startEndVersionFilteredEndpoint<T>(string path)
            => (startTime, endTime, versions) => RequestAndDeserialize<T>(
                appUrl(path,
                    ("start", formatDateTime(startTime)),
                    ("end", formatDateTime(endTime)),
                    ("versions", versionArrayToParameter(versions))
                ));

        private string formatDateTime(DateTimeOffset? time)
            => time?.ToString("O");

        private string versionArrayToParameter(string[] versions)
            => versions == null || versions.Length == 0
                ? null : string.Join("|", versions);

        private string appUrl(string path, params (string name, string value)[] parameters)
            => url($"apps/{owner}/{appName}/{path}", parameters);
    }
}