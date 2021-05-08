using System;
using System.Collections.Generic;
using Verse;

namespace Proton
{
    public static class Context
    {
        public static ProtonSettings settings;

        public static Dictionary<Def, ThingDefSettings> thingSettingsByDef = new Dictionary<Def, ThingDefSettings>();

        public static bool[] thingJunkByDef = new bool[ushort.MaxValue];
    }
}
