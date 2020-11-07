using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RocketMan.Optimizations
{
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
            if (Finder.learning && Finder.statLogging)
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
        internal static MethodBase m_GetValueUnfinalized_Transpiler = AccessTools.Method(typeof(StatWorker_GetValueUnfinalized_Hijacked_Patch), "Transpiler");

        internal static Dictionary<int, Pair<float, int>> cache = new Dictionary<int, Pair<float, int>>(1000);
        internal static Dictionary<int, List<int>> pawnCachedKeys = new Dictionary<int, List<int>>();

        internal static List<int> pawnsCleanupQueue = new List<int>();

        internal static List<Tuple<int, int, float>> requests = new List<Tuple<int, int, float>>();

        private static ThreadStart starter = new ThreadStart(OffMainThreadProcessing);
        private static Thread worker = null;

        private static object locker1 = new object();
        private static object locker2 = new object();
        private static object locker3 = new object();

        [Main.OnDefsLoaded]
        public static void Initialize()
        {
            worker = new Thread(starter);
            worker.Start();
        }

        [Main.OnTick]
        public static void FlushMessages()
        {
            if (!Finder.debug) return;
            lock (locker3)
            {
                while (messages.Count != 0)
                {
                    Monitor.Wait(locker3);
                    Log.Message(messages.Pop());
                }
            }
        }

        internal static Dictionary<int, float> expiryCache = new Dictionary<int, float>();
        internal static List<string> messages = new List<string>();

        internal static int counter = 0;
        internal static int ticker = 0;
        internal static int cleanUps = 0;
        internal static int stage = 0;

        internal static void OffMainThreadProcessing()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep((int)Mathf.Clamp(15 - expiryCache.Count, 0, 15));
                    if (Current.Game == null)
                        continue;
                    if (Find.TickManager.Paused)
                        continue;
                    stage = 1;
                    if (Finder.learning)
                    {
                        if (counter++ % 20 == 0 && expiryCache.Count != 0)
                        {
                            foreach (var unit in expiryCache)
                            {
                                Finder.statExpiry[unit.Key] = (byte)Mathf.Clamp(unit.Value, 0f, 255f);
                                cleanUps++;
                            }
                            expiryCache.Clear();
                        }
                        stage = 2;
                        if (requests.Count > 0)
                        {
                            unsafe
                            {
                                Tuple<int, int, float> request;
                                int timeout = 1024;
                                lock (locker2)
                                {
                                    while (timeout-- > 0) Monitor.Wait(locker2);
                                    if (timeout <= 0)
                                    {
                                        goto ExitBlock;
                                    }
                                    request = requests.Pop();
                                }
                                var statIndex = request.Item1;

                                var deltaT = Mathf.Abs(request.Item2);
                                var deltaX = Mathf.Abs(request.Item3);

                                if (expiryCache.TryGetValue(statIndex, out float value))
                                    expiryCache[statIndex] += Mathf.Clamp(Finder.learningRate * (deltaT / 100 - deltaX * deltaT), -5, 5);
                                else
                                    expiryCache[statIndex] = Finder.statExpiry[statIndex];
                            }
                        ExitBlock:
                            continue;
                        }
                    }
                    stage = 3;
                    while (pawnsCleanupQueue.Count > 0)
                    {
                        lock (locker1)
                        {
                            var pawnIndex = pawnsCleanupQueue.Pop();
                            if (pawnCachedKeys.ContainsKey(pawnIndex))
                                foreach (var key in pawnCachedKeys[pawnIndex])
                                {
                                    cache.RemoveAll(u => u.Key == key);
                                    cleanUps++;
                                }
                        }
                    }
                }
                catch (Exception er)
                {
                    messages.Add(string.Format("ROCKETMAN: error off the main thread in stage {0} with error {1} at {2}", stage, er.Message, er.StackTrace));
                }
                finally
                {
                    if (ticker++ % 128 == 0 && Finder.debug)
                        lock (locker3)
                        {
                            messages.Add(string.Format("ROCKETMAN: off the main thead cleaned {0} and counted {1}", cleanUps, counter));
                            Monitor.Pulse(locker3);
                        }
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
                && !m.DeclaringType.IsAbstract).ToHashSet();

            return methods;
        }

        internal static float UpdateCache(int key, StatWorker statWorker, StatRequest req, bool applyPostProcess, int tick, Pair<float, int> store)
        {
            var value = statWorker.GetValueUnfinalized(req, applyPostProcess);

            if (Finder.statLogging && !Finder.learning)
            {
                Log.Message(string.Format("ROCKETMAN: state {0} for {1} took {2} with key {3}", statWorker.stat.defName, req.thingInt, tick - store.second, key));
            }
            else if (Finder.learning)
            {
                lock (locker2)
                {
                    requests.Add(new Tuple<int, int, float>(statWorker.stat.index, tick - store.second, Mathf.Abs(value - store.first)));
                    Monitor.Pulse(locker2);
                }
            }
            if (req.HasThing && req.Thing is Pawn pawn && pawn != null)
            {
                if (!pawnCachedKeys.TryGetValue(pawn.thingIDNumber, out List<int> keys))
                {
                    pawnCachedKeys[pawn.thingIDNumber] = (keys = new List<int>());
                }
                keys.Add(key);
            }
            cache[key] = new Pair<float, int>(value, tick);
            return value;
        }

        [Main.OnTick]
        public static void CleanCache()
        {
            lock (locker1)
            {
                cache.Clear();
            }
        }

        public static float Replacemant(StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            var tick = GenTicks.TicksGame;

            if (true
                && Finder.enabled
                && Current.Game != null
                && tick >= 600)
            {
                int key = Tools.GetKey(statWorker, req, applyPostProcess);
                Pair<float, int> store;
                lock (locker1)
                {
                    if (!cache.TryGetValue(key, out store))
                    {
                        return UpdateCache(key, statWorker, req, applyPostProcess, tick, store);
                    }
                    if (tick - store.Second > Finder.statExpiry[statWorker.stat.index])
                    {
                        return UpdateCache(key, statWorker, req, applyPostProcess, tick, store);
                    }
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
}
