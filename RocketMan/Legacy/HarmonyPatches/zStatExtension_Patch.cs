using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RocketMan
{
    [StaticConstructorOnStartup]
    public static class StatExtension_Patch
    {
        private static Dictionary<StatRequestModel, CacheableTick<float>> _cache =
            new Dictionary<StatRequestModel, CacheableTick<float>>(StatRequestModelComparer.Instance);

        private static MethodInfo _original = typeof(StatWorker).GetMethod(nameof(StatWorker.GetValue),
            new Type[] { typeof(StatRequest), typeof(bool) });

        private static MethodInfo _prefix =
            typeof(StatExtension_Patch).GetMethod(nameof(Prefix), BindingFlags.Public | BindingFlags.Static);

        private static MethodInfo _postfix =
            typeof(StatExtension_Patch).GetMethod(nameof(Postfix), BindingFlags.Public | BindingFlags.Static);

        private static Stopwatch _stopwatch = new Stopwatch();

        private static int _lastTick = -1;

        private static int _calls = 0;

        private static double _totalTime;

        private static int _cacheHit = 0;

        private static object _lock = new object();

        private static List<string> _record = new List<string>();

        static StatExtension_Patch()
        {
            HarmonyUtility.Instance.Patch(_original, new HarmonyMethod(_prefix), new HarmonyMethod(_postfix));
        }

        public static bool Prefix(StatRequest req, bool applyPostProcess, StatDef ___stat, ref float __result,
            out StatRequestModel __state)
        {
            __state = new StatRequestModel(req, applyPostProcess, ___stat);
            if (_cache.TryGetValue(__state, out var value) && !value.ShouldUpdate(out _))
            {
                return false;
            }

            return true;
        }

        public static void Postfix(StatDef ___stat, ref float __result, StatRequestModel __state)
        {
            if (Current.Game != null)
            {
                if (_cache.TryGetValue(__state, out var value))
                    value.Value = __result;
                else
                    _cache[__state] = MakeCache(__result);
            }
        }

        private static CacheableTick<float> MakeCache(float initValue)
        {
            return new CacheableTick<float>(
                initValue
                , () => Find.TickManager.TicksGame
                , 1
                , null
                , Find.TickManager.TicksGame);
        }
    }
}