using System;
using HarmonyLib;
using RimWorld;
using Verse;
using RocketMan;
using System.Linq;

namespace Gagarin
{

    public static class DefGenerator_Patch
    {
        [Main.OnInitialization]
        public static void Start()
        {
            new GagarinPatchInfo(typeof(GenerateImpliedDefs_PostResolve_Patch)).Patch(Finder.harmony);
        }

        [GagarinPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PostResolve))]
        public static class GenerateImpliedDefs_PostResolve_Patch
        {
            [HarmonyPriority(Priority.First)]
            public static void Postfix()
            {
                Log.Message("GAGARIN: DefGenerator.GenerateImpliedDefs_PostResolve just finished!");

                foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.ToList())
                {
                    if (thingDef == null)
                    {
                        Log.Message($"GARARIN: defName is null for {null}");
                        continue;
                    }
                    if (thingDef.plant == null)
                    {
                        continue;
                    }
                    if (thingDef.blueprintDef != null)
                    {
                        continue;
                    }
                    if (!thingDef.plant.Sowable)
                    {
                        continue;
                    }
                    if (thingDef.plant.harvestedThingDef == null)
                    {
                        continue;
                    }
                    if (thingDef.defName == null)
                    {
                        Log.Message($"GARARIN: defName is null for {thingDef}");
                    }
                }
            }
        }
    }
}
