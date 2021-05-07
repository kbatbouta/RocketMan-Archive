using System.Collections.Generic;
using System.Linq;
using RocketMan;
using Verse;

namespace Soyuz
{
    public static class SoyuzSettingsUtility
    {
        private static List<ThingDef> pawnDefs;

        [Main.OnDefsLoaded]
        public static void LoadSettings()
        {
            pawnDefs = DefDatabase<ThingDef>.AllDefs.Where(def => def.race != null).ToList();
            CacheSettings();
        }

        [Main.OnScribe]
        public static void PostScribe()
        {
            Scribe_Deep.Look(ref Context.settings, "soyuzSettings");
            Finder.soyuzLoaded = true;
        }

        public static void CacheSettings()
        {
            if (Context.settings == null)
                Context.settings = new SoyuzSettings();
            if (Context.settings.raceSettings.Count == 0)
                CreateSettings();
            foreach (var element in Context.settings.raceSettings)
            {
                if (element.pawnDef == null)
                {
                    element.ResolveContent();
                    if (element.pawnDef == null) continue;
                }
                element.Cache();
            }
        }

        public static void CreateSettings()
        {
            Context.settings.raceSettings.Clear();
            foreach (var def in pawnDefs)
            {
                Context.settings.raceSettings.Add(new RaceSettings()
                {
                    pawnDef = def,
                    pawnDefName = def.defName,
                    dilated = def.race.Animal && !def.race.Humanlike && !def.race.IsMechanoid,
                    ignoreFactions = false
                });
            }

            Finder.rocketMod.WriteSettings();
        }
    }
}