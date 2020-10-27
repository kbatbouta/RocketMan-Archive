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
        [HarmonyPatch(typeof(StatPart_ApparelStatOffset), nameof(StatPart_ApparelStatOffset.TransformValue))]
        public static class StatPart_ApparelStatOffSet_Skipper_Patch
        {
            public static CachedDict<int, Dictionary<int, float>> cache = new CachedDict<int, Dictionary<int, float>>();

            public static float currentValue;

            public static int currentKey;

            public static object locker = new object();

            public static bool Skipper(StatPart_ApparelStatOffset instance, StatRequest req, ref float val)
            {
                if (Finder.enabled)
                {
                    if (!req.HasThing || req.Thing == null || !(req.thingInt is Pawn))
                        return false;

                    if (cache.TryGetValue(req.thingInt.thingIDNumber, out var store, expiry: 2500))
                    {
                        lock (locker)
                        {
                            var sub = currentKey = Tools.GetKey(req);
                            var stat = instance.apparelStat ?? instance.parentStat;

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
                    }
                    currentValue = val;
                }
                return true;
            }

            public static void Setter(StatPart_ApparelStatOffset instance, StatRequest req, ref float val)
            {
                if (Finder.enabled)
                {
                    lock (locker)
                    {
                        var key = req.thingInt.thingIDNumber;

                        var sub = Tools.GetKey(req);
                        var stat = instance.apparelStat ?? instance.parentStat;

                        if (sub != currentKey)
                            return;

                        unchecked
                        {
                            sub = HashUtility.HashOne(currentValue.GetHashCode(), sub);
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
            }

            public static void Dirty(Pawn pawn)
            {
                cache.Remove(pawn.thingIDNumber);
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
           ILGenerator generator, MethodBase original)
            {
                return RocketShip.SkipperPatch(instructions, generator, original);
            }
        }
    }
}
