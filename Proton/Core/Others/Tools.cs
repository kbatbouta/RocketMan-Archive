using System;
using Proton.Core;
using Verse;

namespace Proton
{
    public static class Tools
    {
        private static MapJunkTracker[] junkTrackers = new MapJunkTracker[20];

        public static MapJunkTracker GetJunkTracker(this Map map)
        {
            var tracker = junkTrackers[map.Index];
            if (tracker != null && tracker.mapId == map.Index)
                return tracker;
            return junkTrackers[map.Index] = new MapJunkTracker(map);
        }
    }
}
