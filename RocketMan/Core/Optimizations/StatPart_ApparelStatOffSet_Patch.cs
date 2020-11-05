using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using static RocketMan.RocketShip;

namespace RocketMan.Optimizations
{
    [SkipperPatch(typeof(StatPart_ApparelStatOffset), nameof(StatPart_ApparelStatOffset.TransformValue))]
    public static class StatPart_ApparelStatOffSet_Skipper_Patch
    {
        public static CachedDict<int, Dictionary<int, float>> cache = new CachedDict<int, Dictionary<int, float>>();

        public static bool Skipper(StatPart_ApparelStatOffset __instance, ref float __state, StatRequest req, ref float val)
        {
            if (Finder.enabled)
            {
                if (!req.HasThing || req.thingInt == null || !(req.thingInt is Pawn))
                    return false;

                if (cache.TryGetValue(req.thingInt.thingIDNumber, out var store, expiry: 2500))
                {
                    var sub = Tools.GetKey(req);
                    var stat = __instance.apparelStat ?? __instance.parentStat;

                    unchecked
                    {
                        sub = HashUtility.HashOne(val.GetHashCode(), sub);
                        sub = HashUtility.HashOne(stat.index, sub);
                    }

                    if (store.TryGetValue(sub, out var value))
                    {
                        val = value;
                        return false;
                    }
                }
                __state = val;
            }
            return true;
        }

        public static void Setter(StatPart_ApparelStatOffset __instance, ref float __state, StatRequest req, ref float val)
        {
            if (Finder.enabled)
            {
                var key = req.thingInt.thingIDNumber;

                var sub = Tools.GetKey(req);
                var stat = __instance.apparelStat ?? __instance.parentStat;

                unchecked
                {
                    sub = HashUtility.HashOne(__state.GetHashCode(), sub);
                    sub = HashUtility.HashOne(stat.index, sub);
                }

                if (cache.TryGetValue(key, out var store))
                {
                    store[sub] = val;
                }
                else
                {
                    cache[key] = new Dictionary<int, float>();
                    cache[key][sub] = val;
                }
            }
        }

        public static void Dirty(Pawn pawn)
        {
            cache.Remove(pawn.thingIDNumber);
        }
    }
}
