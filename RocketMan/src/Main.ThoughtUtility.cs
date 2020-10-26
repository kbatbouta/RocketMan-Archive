using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RocketMan
{
    public partial class Main
    {
        [HarmonyPatch(typeof(ThoughtUtility), nameof(ThoughtUtility.NullifyingTrait))]
        public static class ThoughtUtility_NullifyingTrait_Patch
        {
            private static CachedDict<int, Trait> cache = new CachedDict<int, Trait>();

            public static bool Prefix(ThoughtDef def, Pawn pawn, ref Trait __result, out bool __state)
            {
                if (def.nullifyingTraits == null)
                {
                    __result = null;
                    return (__state = false);
                }

                if (Finder.enabled && Finder.thoughtsCaching)
                {
                    if (cache.TryGetValue(Tools.GetKey(def, pawn), out __result, out __state, expiry: 2500))
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    return !(__state = false);
                }
            }

            public static void Postfix(ThoughtDef def, Pawn pawn, Trait __result, bool __state)
            {
                if (__state == false)
                {
                    return;
                }

                cache[Tools.GetKey(def, pawn)] = __result;
            }
        }

        [HarmonyPatch(typeof(ThoughtUtility), nameof(ThoughtUtility.NullifyingTale))]
        public static class ThoughtUtility_NullifyingTale_Patch
        {
            private static CachedDict<int, TaleDef> cache = new CachedDict<int, TaleDef>();

            public static bool Prefix(ThoughtDef def, Pawn pawn, ref TaleDef __result, out bool __state)
            {
                if (def.nullifyingOwnTales == null)
                {
                    __result = null;
                    return (__state = false);
                }

                if (Finder.enabled && Finder.thoughtsCaching)
                {
                    if (cache.TryGetValue(Tools.GetKey(def, pawn), out __result, out __state, 2500))
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    return !(__state = false);
                }
            }

            public static void Postfix(ThoughtDef def, Pawn pawn, TaleDef __result, bool __state)
            {
                if (__state == false)
                {
                    return;
                }

                cache[Tools.GetKey(def, pawn)] = __result;
            }
        }

        [HarmonyPatch(typeof(ThoughtUtility), nameof(ThoughtUtility.NullifyingHediff))]
        public static class ThoughtUtility_NullifyingHediff_Patch
        {
            private static CachedDict<int, Hediff> cache = new CachedDict<int, Hediff>();
            private static Dictionary<int, List<int>> pawn_keys = new Dictionary<int, List<int>>();

            public static bool Prefix(ThoughtDef def, Pawn pawn, ref Hediff __result, out bool __state)
            {
                if (def.IsMemory == true)
                {
                    __result = null;
                    return (__state = false);
                }

                if (Finder.enabled && Finder.thoughtsCaching)
                {
                    if (cache.TryGetValue(Tools.GetKey(def, pawn), out __result, out __state, 500))
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    return !(__state = false);
                }
            }

            public static void Postfix(ThoughtDef def, Pawn pawn, Hediff __result, bool __state)
            {
                if (__state == false)
                {
                    return;
                }
                var key = Tools.GetKey(def, pawn);
                cache[key] = __result;
                if (pawn_keys.TryGetValue(pawn.thingIDNumber, out var store))
                    store.Add(key);
                else
                {
                    pawn_keys[pawn.thingIDNumber] = new List<int>() { key };
                }
            }

            public static void Dirty(Pawn pawn)
            {
                if (pawn_keys.TryGetValue(pawn.thingIDNumber, out var store))
                {
                    foreach (var key in store)
                    {
                        cache.Remove(key);
                    }
                    store.Clear();
                }
            }
        }
    }
}
