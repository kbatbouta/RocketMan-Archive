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
            if (Context.settings.thingDefSettings.Count == 0)
                CreateSettings();
            foreach (var element in Context.settings.thingDefSettings)
            {
                if (element.thingDef == null)
                {
                    element.ResolveContent();
                    if (element.thingDef == null) continue;
                }
                element.Cache();
            }
        }

        public static void CreateSettings()
        {
            Context.settings.thingDefSettings.Clear();
            foreach (var def in thingDefs)
            {
                Context.settings.thingDefSettings.Add(new ThingDefSettings()
                {
                    thingDef = def,
                    thingDefName = def.defName,
                    isCritical = true,
                    isRecyclable = false
                });
            }

            Finder.rocketMod.WriteSettings();
        }
    }
}
