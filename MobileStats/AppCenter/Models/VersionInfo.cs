namespace MobileStats.AppCenter.Models
{
    class VersionInfo
    {
        public string AppVersionId { get; set; }
        public string AppId { get; set; }
        public string DisplayName { get; set; }
        public string AppVersion { get; set; }
        public int BuildNumber { get; set; }

        public override string ToString()
            => $"{AppVersion} ({BuildNumber})";
    }
}
