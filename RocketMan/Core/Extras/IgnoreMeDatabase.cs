using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RocketMan
{
    public static class IgnoreMeDatabase
    {
        private static bool[] defsIgnored = new bool[ushort.MaxValue];

        private static HashSet<string> parsedDefNames = new HashSet<string>();

        public static bool ShouldIgnore(Def def) => defsIgnored[def.index];

        private static string report = "ROCKETMAN: <color=red>IgnoreMe report</color>";

        public static void Add(Def def)
        {
            report += $"\nROCKETMAN: IgnoreMe add def by name:{def.defName}";
            defsIgnored[def.index] = true;
        }

        public static void Add(string defName) => parsedDefNames.Add(defName);

        public static void AddAll(ModContentPack mod)
        {
            foreach (Def def in mod.AllDefs) Add(def);
        }

        public static void AddPackageId(string packageId)
        {
            packageId = packageId.ToLower();
            try { AddAll(LoadedModManager.runningMods.First(m => m.PackageId.ToLower() == packageId)); }
            catch (Exception) { report += $"\nROCKETMAN: Failed to find mod with packageId {packageId}"; }
        }

        public static void Prepare()
        {
            foreach (string defName in parsedDefNames)
            {
                try
                {
                    if (DefDatabase<ThingDef>.defsByName.TryGetValue(defName, out ThingDef thingDef))
                    {
                        Add(thingDef);
                        continue;
                    }
                    if (DefDatabase<StatDef>.defsByName.TryGetValue(defName, out StatDef statDef))
                    {
                        Add(statDef);
                        continue;
                    }
                    if (DefDatabase<HediffDef>.defsByName.TryGetValue(defName, out HediffDef hediffDef))
                    {
                        Add(hediffDef);
                        continue;
                    }
                    if (DefDatabase<BuildableDef>.defsByName.TryGetValue(defName, out BuildableDef buildableDef))
                    {
                        Add(buildableDef);
                        continue;
                    }
                    if (DefDatabase<BodyDef>.defsByName.TryGetValue(defName, out BodyDef bodyDef))
                    {
                        Add(bodyDef);
                        continue;
                    }
                }
                catch (Exception er)
                {
                    Log.Warning($"ROCKETMAN: Parsing IgnoreMe rule failed by name {defName} with error {er}");
                }
            }
            Log.Message(report);
        }
    }
}
