using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RocketLite.Optimizations
{
    internal static class StatWorker_GetValueUnfinalized_Hijacked_Patch
    {
        internal static MethodBase m_GetValueUnfinalized = AccessTools.Method(typeof(StatWorker), "GetValueUnfinalized",
            new[] { typeof(StatRequest), typeof(bool) });

        internal static MethodBase m_GetValueUnfinalized_Replacemant =
            AccessTools.Method(typeof(StatWorker_GetValueUnfinalized_Hijacked_Patch), "Replacemant");

        internal static MethodBase m_GetValueUnfinalized_Transpiler =
            AccessTools.Method(typeof(StatWorker_GetValueUnfinalized_Hijacked_Patch), "Transpiler");

        internal static Dictionary<int, int> signatures = new Dictionary<int, int>();

        internal static Dictionary<int, Tuple<float, int, int>> cache =
            new Dictionary<int, Tuple<float, int, int>>(1000);

        internal static List<Tuple<int, int, float>> requests = new List<Tuple<int, int, float>>();

        internal static Dictionary<int, float> expiryCache = new Dictionary<int, float>();
        internal static List<string> messages = new List<string>();

        internal static int counter;
        internal static int cleanUps;

        private static Stopwatch expiryStopWatch = new Stopwatch();

        internal static void ProcessExpiryCache()
        {
            if (requests.Count == 0 || Find.TickManager == null)
                return;
            expiryStopWatch.Reset();
            expiryStopWatch.Start();

            while (requests.Count > 0 && expiryStopWatch.ElapsedMilliseconds <= 1)
            {
                Tuple<int, int, float> request;
                requests.Pop();
            }
            expiryStopWatch.Stop();
        }

        public static void Dirty(Pawn pawn)
        {
            var signature = pawn.GetSignature(true);
        }

        internal static IEnumerable<MethodBase> TargetMethodsUnfinalized()
        {
            yield return AccessTools.Method(typeof(BeautyUtility), "CellBeauty");
            yield return AccessTools.Method(typeof(BeautyUtility), "AverageBeautyPerceptible");
            yield return AccessTools.Method(typeof(StatExtension), "GetStatValue");
            yield return AccessTools.Method(typeof(StatWorker), "GetValue", new[] { typeof(StatRequest), typeof(bool) });

            foreach (var type in typeof(StatWorker).AllSubclassesNonAbstract())
                yield return AccessTools.Method(type, "GetValue", new[] { typeof(StatRequest), typeof(bool) });

            foreach (var type in typeof(StatPart).AllSubclassesNonAbstract())
                yield return AccessTools.Method(type, "TransformValue", new[] { typeof(StatRequest), typeof(float).MakeByRefType() });

            foreach (var type in typeof(StatExtension).AllSubclassesNonAbstract())
            {
                yield return AccessTools.Method(type, "GetStatValue");
                yield return AccessTools.Method(type, "GetStatValueAbstract");
            }
        }

        public static IEnumerable<MethodBase> GetTargetMethods()
        {
            var methods = TargetMethodsUnfinalized().Where(m => true
                                                                && m != null
                                                                && !m.IsAbstract
                                                                && m.HasMethodBody()
                                                                && !m.DeclaringType.IsAbstract).ToHashSet();

            return methods;
        }

        public static float UpdateCache(int key, StatWorker statWorker, StatRequest req, bool applyPostProcess,
            int tick, Tuple<float, int, int> store)
        {
            var value = statWorker.GetValueUnfinalized(req, applyPostProcess);
            cache[key] = new Tuple<float, int, int>(value, tick, req.thingInt?.GetSignature() ?? -1);
            return value;
        }

        public static float Replacemant(StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            var tick = GenTicks.TicksGame;
            if (true
                && Current.Game != null
                && tick >= 600)
            {
                var key = Tools.GetKey(statWorker, req, applyPostProcess);
                var signature = req.thingInt?.GetSignature() ?? -1;

                if (!cache.TryGetValue(key, out var store))
                    return UpdateCache(key, statWorker, req, applyPostProcess, tick, store);

                if (tick - store.Item2 - 1 > 5 || signature != store.Item3)
                    return UpdateCache(key, statWorker, req, applyPostProcess, tick, store);
                return store.Item1;
            }
            return statWorker.GetValueUnfinalized(req, applyPostProcess);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(m_GetValueUnfinalized, m_GetValueUnfinalized_Replacemant);
        }
    }
}