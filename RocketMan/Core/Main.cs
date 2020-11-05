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
using UnityEngine.Assertions.Must;
using RimWorld.Planet;

namespace RocketMan
{
    [StaticConstructorOnStartup]
    public partial class Main : ModBase
    {
        [AttributeUsage(System.AttributeTargets.Method)]
        public class OnDefsLoaded : Attribute
        {
        }

        [AttributeUsage(System.AttributeTargets.Method)]
        public class OnTickLong : Attribute
        {
        }


        [AttributeUsage(System.AttributeTargets.Method)]
        public class OnTick : Attribute
        {
        }

        [AttributeUsage(System.AttributeTargets.Method)]
        public class OnEarlyInitialize : Attribute
        {
        }

        [AttributeUsage(System.AttributeTargets.Method)]
        public class OnMapComponentsInitializing : Attribute
        {
        }

        [AttributeUsage(System.AttributeTargets.Method)]
        public class OnClearCache : Attribute
        {
        }

        [Main.OnDefsLoaded]
        public static void Initialization()
        {
            Finder.harmony.PatchAll();
            Finder.rocket.PatchAll();
        }

        public static IEnumerable<Action> GetActions<T>() where T : Attribute
        {
            foreach (var method in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .SelectMany(t => t.GetMethods())
                .Where(m => m.TryGetAttribute<T>(out var _))
                .ToArray())
            {
                Log.Message(string.Format("ROCKETMAN: Found method with attribute {0}, {1}:{2}", typeof(T).Name, method.DeclaringType.Name, method.Name));
                yield return () => { method.Invoke(null, null); };
            }
        }

        public List<Action> onMapComponentsInitializing = GetActions<OnMapComponentsInitializing>().ToList();
        public List<Action> onEarlyInitialize = GetActions<OnEarlyInitialize>().ToList();
        public List<Action> onClearCache = GetActions<OnClearCache>().ToList();
        public List<Action> onTick = GetActions<OnTick>().ToList();
        public List<Action> onTickLong = GetActions<OnTickLong>().ToList();
        public List<Action> onDefsLoaded = GetActions<OnDefsLoaded>().ToList();

        public override void MapComponentsInitializing(Map map)
        {
            base.MapComponentsInitializing(map);

            for (int i = 0; i < onMapComponentsInitializing.Count; i++)
            {
                onMapComponentsInitializing[i].Invoke();
            }
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();

            for (int i = 0; i < onDefsLoaded.Count; i++)
            {
                onDefsLoaded[i].Invoke();
            }
        }

        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);

            if (currentTick % Finder.universalCacheAge != 0) return;

            for (int i = 0; i < onTick.Count; i++)
            {
                onTick[i].Invoke();
            }

            if (currentTick % (Finder.universalCacheAge * 5) != 0) return;

            for (int i = 0; i < onTickLong.Count; i++)
            {
                onTickLong[i].Invoke();
            }
        }

        public override void EarlyInitialize()
        {
            base.EarlyInitialize();

            for (int i = 0; i < onEarlyInitialize.Count; i++)
            {
                onEarlyInitialize[i].Invoke();
            }
        }

        public void ClearCache()
        {
            for (int i = 0; i < onClearCache.Count; i++)
            {
                onClearCache[i].Invoke();
            }
        }
    }
}