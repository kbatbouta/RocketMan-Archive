using System;
using HugsLib;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using System.CodeDom;
using System.Threading;
using System.Diagnostics;

namespace RocketMan.src
{
    [StaticConstructorOnStartup]
    public class Main : ModBase
    {
        public static Action[] onMapComponentsInitializing = new Action[]
        {

        };

        public static Action[] onEarlyInitialize = new Action[]
        {

        };

        public static Action[] onClearCache = new Action[]
        {

        };

        public static Action[] onTick = new Action[]
        {
            () => StatWorker_GetValueUnfinalized_Hijacked_Patch.CleanCache()
        };

        public static Action[] onDefsLoaded = new Action[]
        {
            () => RocketMod.UpdateStats(),
            () => StatWorker_GetValueUnfinalized_Hijacked_Patch.Initialize()
        };

        public override void MapComponentsInitializing(Map map)
        {
            base.MapComponentsInitializing(map);

            for (int i = 0; i < onMapComponentsInitializing.Length; i++)
            {
                onMapComponentsInitializing[i].Invoke();
            }
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();

            for (int i = 0; i < onDefsLoaded.Length; i++)
            {
                onDefsLoaded[i].Invoke();
            }
        }

        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);

            if (currentTick % Finder.universalCacheAge != 0) return;

            for (int i = 0; i < onTick.Length; i++)
            {
                onTick[i].Invoke();
            }
        }

        public override void EarlyInitialize()
        {
            base.EarlyInitialize();

            for (int i = 0; i < onEarlyInitialize.Length; i++)
            {
                onEarlyInitialize[i].Invoke();
            }
        }

        public void ClearCache()
        {
            for (int i = 0; i < onClearCache.Length; i++)
            {
                onClearCache[i].Invoke();
            }
        }

        [HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized", new[] { typeof(StatRequest), typeof(bool) })]
        internal static class StatWorker_GetValueUnfinalized_Interrupt_Patch
        {
            public static HashSet<MethodBase> callingMethods = new HashSet<MethodBase>();

            public static MethodBase m_Interrupt = AccessTools.Method(typeof(StatWorker_GetValueUnfinalized_Interrupt_Patch), "Interrupt");

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Ldarg_2);
                yield return new CodeInstruction(OpCodes.Call, m_Interrupt);

                foreach (CodeInstruction code in instructions)
                    yield return code;
            }

            public static void Interrupt(StatWorker statWorker, StatRequest req, bool applyPostProcess)
            {
                if (Finder.learning && Finder.debug)
                {
                    StackTrace trace = new StackTrace();
                    StackFrame frame = trace.GetFrame(2);
                    MethodBase method = frame.GetMethod();

                    String handler = method.GetStringHandler();

                    Log.Message(string.Format("ROCKETMAN: called stats.GetUnfinalizedValue from {0}", handler));

                    callingMethods.Add(method);
                }
            }
        }

        [HarmonyPatch]
        internal static class StatWorker_GetValueUnfinalized_Hijacked_Patch
        {
            internal static MethodBase m_GetValueUnfinalized = AccessTools.Method(typeof(StatWorker), "GetValueUnfinalized", new[] { typeof(StatRequest), typeof(bool) });

            internal static MethodBase m_GetValueUnfinalized_Replacemant = AccessTools.Method(typeof(StatWorker_GetValueUnfinalized_Hijacked_Patch), "Replacemant");

            internal static MethodBase m_GetValueUnfinalized_Transpiler = AccessTools.Method(typeof(Main.StatWorker_GetValueUnfinalized_Hijacked_Patch), "Transpiler");

            internal static Dictionary<int, Pair<float, int>> cache = new Dictionary<int, Pair<float, int>>(1000);

            internal static Dictionary<int, List<int>> pawnCachedKeys = new Dictionary<int, List<int>>();

            internal static List<int> pawnsCleanupQueue = new List<int>();

            internal static List<Tuple<int, int, float>> requests = new List<Tuple<int, int, float>>();


            private static ThreadStart starter = null;
            private static Thread worker = null;

            public static void Initialize()
            {
                starter = new ThreadStart(OffMainThreadProcessing);
                worker = new Thread(starter);
                worker.Start();
            }

            internal static void OffMainThreadProcessing()
            {
                Dictionary<int, float> expiryCache = new Dictionary<int, float>();

                float counter = 0;

                while (true)
                {
                    try
                    {
                        Thread.Sleep((int)Mathf.Clamp(15 - expiryCache.Count, 0, 15));

                        if (Current.Game == null)
                        {
                            continue;
                        }

                        if (Find.TickManager.Paused)
                        {
                            continue;
                        }

                        if (Finder.learning)
                        {
                            if (counter++ % 20 == 0 && expiryCache.Count != 0)
                            {
                                foreach (var unit in expiryCache)
                                {
                                    Finder.statExpiry[unit.Key] = (byte)Mathf.Clamp(unit.Value, 0f, 255f);
                                }

                                expiryCache.Clear();
                            }

                            if (requests.Count > 0)
                            {
                                var request = requests.Pop();

                                var statIndex = request.Item1;

                                var deltaT = request.Item2;
                                var deltaX = request.Item3;

                                if (expiryCache.TryGetValue(statIndex, out float value))
                                {
                                    expiryCache[statIndex] += Finder.learningRate * (128 - deltaX * deltaT);
                                }
                                else
                                {
                                    expiryCache[statIndex] = Finder.statExpiry[statIndex];
                                }
                            }
                        }

                        while (pawnsCleanupQueue.Count > 0)
                        {
                            var pawnIndex = pawnsCleanupQueue.Pop();

                            if (pawnCachedKeys.ContainsKey(pawnIndex))
                            {
                                foreach (var key in pawnCachedKeys[pawnIndex])
                                {
                                    cache.RemoveAll(u => u.Key == key);
                                }
                            }
                        }
                    }
                    catch (Exception er)
                    {

                    }
                    finally
                    {

                    }
                }
            }

            internal static IEnumerable<MethodBase> TargetMethodsUnfinalized()
            {
                yield return AccessTools.Method(typeof(BeautyUtility), "CellBeauty");
                yield return AccessTools.Method(typeof(BeautyUtility), "AverageBeautyPerceptible");
                yield return AccessTools.Method(typeof(StatExtension), "GetStatValue");
                yield return AccessTools.Method(typeof(StatWorker), "GetValue", new[] { typeof(StatRequest), typeof(bool) });

                foreach (Type type in typeof(StatWorker).AllSubclassesNonAbstract())
                {
                    yield return AccessTools.Method(type, "GetValue", new[] { typeof(StatRequest), typeof(bool) });
                }

                foreach (Type type in typeof(StatPart).AllSubclassesNonAbstract())
                {
                    yield return AccessTools.Method(type, "TransformValue");
                }

                foreach (Type type in typeof(StatExtension).AllSubclassesNonAbstract())
                {
                    yield return AccessTools.Method(type, "GetStatValue");
                    yield return AccessTools.Method(type, "GetStatValueAbstract");
                }
            }

            internal static IEnumerable<MethodBase> TargetMethods()
            {
                var methods = TargetMethodsUnfinalized().Where(m => true
                   && m != null
                   && !m.IsAbstract
                   && !m.DeclaringType.IsAbstract);

                return methods;
            }

            internal static float UpdateCache(int key, StatWorker statWorker, StatRequest req, bool applyPostProcess, int tick, Pair<float, int> store)
            {
                var value = statWorker.GetValueUnfinalized(req, applyPostProcess);

                if (Finder.debug && !Finder.learning)
                {
                    Log.Message(string.Format("ROCKETMAN: state {0} for {1} took {2} with key {3}", statWorker.stat.defName, req.thingInt, tick - store.second, key));
                }

                if (Finder.learning)
                {
                    requests.Add(new Tuple<int, int, float>(statWorker.stat.index, tick - store.second, Mathf.Abs(value - store.first)));
                }

                if (req.HasThing && req.Thing is Pawn pawn && pawn != null)
                {
                    List<int> keys = null;
                    if (!pawnCachedKeys.TryGetValue(pawn.thingIDNumber, out keys))
                        pawnCachedKeys[pawn.thingIDNumber] = (keys = new List<int>());
                    keys.Add(key);
                }

                cache[key] = new Pair<float, int>(value, tick);
                return value;
            }

            public static void CleanCache()
            {
                cache.Clear();
            }

            public static float Replacemant(StatWorker statWorker, StatRequest req, bool applyPostProcess)
            {
                if (Finder.enabled && Current.Game != null)
                {
                    var key = Tools.GetKey(statWorker, req, applyPostProcess);

                    var tick = GenTicks.TicksGame;

                    if (!cache.TryGetValue(key, out var store))
                    {
                        return UpdateCache(key, statWorker, req, applyPostProcess, tick, store);
                    }

                    if (tick - store.Second > Finder.statExpiry[statWorker.stat.index])
                    {
                        return UpdateCache(key, statWorker, req, applyPostProcess, tick, store);
                    }

                    return store.First;
                }
                else
                {
                    return statWorker.GetValueUnfinalized(req, applyPostProcess);
                }
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return Transpilers.MethodReplacer(instructions, m_GetValueUnfinalized, m_GetValueUnfinalized_Replacemant);
            }
        }

        [HarmonyPatch]
        public static class StatPart_ApparelStatOffSet_Patch
        {
            internal static Dictionary<int, Pair<Dictionary<ushort, float>, int>> cache = new Dictionary<int, Pair<Dictionary<ushort, float>, int>>();

            [HarmonyPatch(typeof(StatPart_ApparelStatOffset), nameof(StatPart_ApparelStatOffset.TransformValue))]
            [HarmonyPriority(9999)]
            [HarmonyPrefix]
            public static bool TransformValue_Prefix(StatPart_ApparelStatOffset __instance, StatRequest req, ref float val, out Pair<float, bool> __state)
            {
                if (Finder.enabled)
                {
                    if (!req.HasThing || req.Thing == null || !(req.thingInt is Pawn))
                    {
                        __state = new Pair<float, bool>(val, true);
                        return false;
                    }

                    var key = Tools.GetKey(req);
                    var tick = GenTicks.TicksGame;
                    var stat = __instance.apparelStat ?? __instance.parentStat;
                    var subKey = 0;

                    unchecked
                    {
                        subKey = HashUtility.HashOne(val.GetHashCode());
                        subKey = HashUtility.HashOne(stat.index, subKey);
                    }

                    if (cache.TryGetValue(key, out var store) && tick - store.second < 2500)
                    {
                        if (store.first.TryGetValue((ushort)subKey, out float value))
                        {
                            __state = new Pair<float, bool>(val = value, false);
                            return false;
                        }
                    }

                    __state = new Pair<float, bool>(val, true);
                    return true;
                }
                else
                {
                    __state = new Pair<float, bool>(val, false);
                    return true;
                }
            }

            [HarmonyPatch(typeof(StatPart_ApparelStatOffset), nameof(StatPart_ApparelStatOffset.TransformValue))]
            [HarmonyPostfix]
            public static void TransformValue_Postfix(StatPart_ApparelStatOffset __instance, StatRequest req, ref float val, Pair<float, bool> __state)
            {
                if (!__state.second || !Finder.enabled)
                {
                    return;
                }

                if (!req.HasThing || req.Thing == null || !(req.thingInt is Pawn))
                {
                    return;
                }

                var key = Tools.GetKey(req);
                var tick = GenTicks.TicksGame;
                var stat = __instance.apparelStat ?? __instance.parentStat;
                var subKey = 0;

                unchecked
                {
                    subKey = HashUtility.HashOne(__state.first.GetHashCode());
                    subKey = HashUtility.HashOne(stat.index, subKey);
                }

                if (cache.TryGetValue(key, out var store) && tick - store.second < 2500)
                {
                    store.first[(ushort)subKey] = val;
                }
                else
                {
                    Dictionary<ushort, float> dict;

                    if (store == null || store.first == null)
                    {
                        dict = new Dictionary<ushort, float>();
                    }
                    else
                    {
                        store.first.Clear();
                        dict = store.first;
                    }

                    dict[(ushort)subKey] = val;
                    cache[key] = new Pair<Dictionary<ushort, float>, int>(dict, tick);
                }
            }

            [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelAdded))]
            [HarmonyPostfix]
            public static void Notify_ApparelAdded_Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
            {
                var key = __instance.pawn.thingIDNumber;

                cache.RemoveAll(t => t.Key == __instance.pawn.thingIDNumber);
            }

            [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelRemoved))]
            [HarmonyPostfix]
            public static void Notify_ApparelRemoved_Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
            {
                var key = __instance.pawn.thingIDNumber;

                cache.RemoveAll(t => t.Key == __instance.pawn.thingIDNumber);
            }

            [HarmonyPatch(typeof(Pawn), nameof(Pawn.Destroy))]
            [HarmonyPostfix]
            public static void Destroy_Postfix(Pawn __instance)
            {
                var key = __instance.thingIDNumber;

                cache.RemoveAll(t => t.Key == __instance.thingIDNumber);
            }
        }

        [HarmonyPatch]
        public static class Pawn_ApparelTracker_Patch
        {
            [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_LostBodyPart))]
            [HarmonyPostfix]
            public static void Notify_LostBodyPart_Postfix(Pawn_ApparelTracker __instance)
            {
                var key = __instance.pawn.thingIDNumber;

                StatPart_ApparelStatOffSet_Patch.cache.RemoveAll(t => t.Key == __instance.pawn.thingIDNumber);
                StatWorker_GetValueUnfinalized_Hijacked_Patch.pawnsCleanupQueue.Add(__instance.pawn.thingIDNumber);
            }

            [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.ApparelChanged))]
            [HarmonyPostfix]
            public static void Notify_ApparelChanged_Postfix(Pawn_ApparelTracker __instance)
            {
                var key = __instance.pawn.thingIDNumber;

                StatPart_ApparelStatOffSet_Patch.cache.RemoveAll(t => t.Key == __instance.pawn.thingIDNumber);
                StatWorker_GetValueUnfinalized_Hijacked_Patch.pawnsCleanupQueue.Add(__instance.pawn.thingIDNumber);
            }
        }
    }
}
