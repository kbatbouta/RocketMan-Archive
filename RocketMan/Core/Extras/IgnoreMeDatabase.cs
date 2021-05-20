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

        public static void ParsePrepare()
        {
            try
            {
                Dictionary<string, ThingDef> thingDefsByName =
                    DefDatabase<ThingDef>.defsByName;
                Dictionary<string, StatDef> statDefsByName =
                    DefDatabase<StatDef>.defsByName;
                Dictionary<string, HediffDef> hediffDefsByName =
                    DefDatabase<HediffDef>.defsByName;
                Dictionary<string, BuildableDef> buildableDefsByName =
                    DefDatabase<BuildableDef>.defsByName;
                Dictionary<string, BodyDef> bodyDefsByName =
                    DefDatabase<BodyDef>.defsByName;
                Dictionary<string, JobDef> jobsDefsByName =
                    DefDatabase<JobDef>.defsByName;
                foreach (string defName in parsedDefNames)
                {
                    try
                    {
                        if (thingDefsByName.TryGetValue(defName, out ThingDef thingDef))
                        {
                            Add(thingDef);
                            continue;
                        }
                        if (statDefsByName.TryGetValue(defName, out StatDef statDef))
                        {
                            Add(statDef);
                            continue;
                        }
                        if (hediffDefsByName.TryGetValue(defName, out HediffDef hediffDef))
                        {
                            Add(hediffDef);
                            continue;
                        }
                        if (buildableDefsByName.TryGetValue(defName, out BuildableDef buildableDef))
                        {
                            Add(buildableDef);
                            continue;
                        }
                        if (bodyDefsByName.TryGetValue(defName, out BodyDef bodyDef))
                        {
                            Add(bodyDef);
                            continue;
                        }
                        if (jobsDefsByName.TryGetValue(defName, out JobDef jobDef))
                        {
                            Add(jobDef);
                            continue;
                        }
                    }
                    catch (Exception er) { Log.Warning($"ROCKETMAN: Parsing IgnoreMe rule failed by name {defName} with error {er}"); }
                }
                // ------------------------------
                // Publish report to avoid spam..
                if (RocketDebugPrefs.debug) Log.Message(report);
            }
            catch (Exception er) { Log.Error($"ROCKETRULES: Parsing error! {er}"); }
        }
    }
}
