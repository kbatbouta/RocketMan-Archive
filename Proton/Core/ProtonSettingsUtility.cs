using System;
using System.Collections.Generic;
using System.Linq;
using RocketMan;
using Verse;

namespace Proton
{
    public static class ProtonSettingsUtility
    {
        private static List<ThingDef> thingDefs;

        [Main.OnScribe]
        public static void PostScribe()
        {
            Scribe_Deep.Look(ref Context.settings, "protonSettings");
            Finder.protonLoaded = true;
        }

        [Main.OnDefsLoaded]
        public static void LoadSettings()
        {
            thingDefs = DefDatabase<ThingDef>.AllDefs.ToList();
            CacheSettings();
        }

        public static void CacheSettings()
        {
            if (Context.settings == null)
                Context.settings = new ProtonSettings();
        }

        public static void CreateSettings()
        {
        }
    }
}
