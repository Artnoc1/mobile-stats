namespace MobileStats.AppCenter.Models
{
    class VersionCount
    {
        public string Version { get; set; }
        public int Count { get; set; }
        public int PreviousCount { get; set; }

        public override string ToString() => $"{Version} with {Count} devices";
    }
}
