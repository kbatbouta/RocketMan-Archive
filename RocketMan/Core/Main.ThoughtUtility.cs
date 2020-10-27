using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RocketMan
{
    public partial class Main
    {
        [HarmonyPatch(typeof(ThoughtUtility), nameof(ThoughtUtility.NullifyingHediff))]
        public static class ThoughtUtility_NullifyingHediff_Patch
        {
            public static CachedDict<int, Hediff> cache = new CachedDict<int, Hediff>();
            public static Dictionary<Pawn, List<int>> cachedKeys = new Dictionary<Pawn, List<int>>();

            public static bool Skipper(ref Hediff result, ThoughtDef def, Pawn pawn)
            {
                if (Finder.enabled && Finder.thoughtsCaching)
                {
                    result = null;
                    var key = Tools.GetKey(def, pawn);
                    if (cache.TryGetValue(key, out var value, expiry: 2500))
                    {
                        result = value;
                        return false;
                    }
                }
                return true;
            }

            public static void Setter(ref Hediff result, ThoughtDef def, Pawn pawn)
            {
                if (Finder.enabled && Finder.thoughtsCaching)
                {
                    var key = Tools.GetKey(def, pawn);
                    cache[key] = result;
                    if (cachedKeys.TryGetValue(pawn, out List<int> store))
                        store.Add(key);
                    else
                    {
                        cachedKeys[pawn] = new List<int>() { key };
                    }
                }
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator, MethodBase original)
            {
                return RocketShip.SkipperPatch(instructions, generator, original);
            }
        }

        [HarmonyPatch(typeof(ThoughtUtility), nameof(ThoughtUtility.NullifyingTrait))]
        public static class ThoughtUtility_NullifyingTrait_Patch
        {
            public static CachedDict<int, Trait> cache = new CachedDict<int, Trait>();

            public static bool Skipper(ref Trait result, ThoughtDef def, Pawn pawn)
            {
                if (Finder.enabled && Finder.thoughtsCaching)
                {
                    result = null;
                    if (cache.TryGetValue(Tools.GetKey(def, pawn), out var value, expiry: 2500))
                    {
                        result = value;
                        return false;
                    }
                }
                return true;
            }

            public static void Setter(ref Trait result, ThoughtDef def, Pawn pawn)
            {
                if (Finder.enabled && Finder.thoughtsCaching)
                {
                    cache[Tools.GetKey(def, pawn)] = result;
                }
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator, MethodBase original)
            {
                return RocketShip.SkipperPatch(instructions, generator, original);
            }
        }
    }
}