using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MobileStats.AppCenter
{
    static class AppNames
    {
        public static ReadOnlyDictionary<string, (string Name, string Emoji, string AppId)> KnownAppNames { get; }
            = new ReadOnlyDictionary<string, (string, string, string)>(
                new Dictionary<string, (string, string, string)>
                {
                    ["Toggl-iOS"] = ("iOS", ":daneel:", "com.toggl.daneel"),
                    ["Toggl-Android"] = ("Android", ":giskard:", "com.toggl.giskard"),
                });
            
    }
}