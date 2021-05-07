using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HugsLib;
using Verse;

namespace RocketMan
{
    [StaticConstructorOnStartup]
    public class Main : ModBase
    {
        private static List<Action> onClearCache = GetActions<OnClearCache>().ToList();
        private static List<Action> onDefsLoaded = GetActions<OnDefsLoaded>().ToList();

        private static List<Action> onMapComponentsInitializing = GetActions<OnMapComponentsInitializing>().ToList();
        private static List<Action> onTick = GetActions<OnTick>().ToList();
        private static List<Action> onTickLong = GetActions<OnTickLong>().ToList();

        public static List<Action> onStaticConstructors;
        public static List<Action> onInitialization;
        public static List<Action> onScribe;

        public static IEnumerable<Action> GetActions<T>() where T : Attribute
        {
            foreach (var method in AppDomain.CurrentDomain.GetAssemblies()
                .Where(ass => !ass.FullName.Contains("System") && !ass.FullName.Contains("VideoTool"))
                .SelectMany(a => a.GetTypes())
                .SelectMany(t => t.GetMethods())
                .Where(m => m.TryGetAttribute<T>(out var _))
                .ToArray())
            {
                Log.Message(string.Format("ROCKETMAN: Found method with attribute {0}, {1}:{2}", typeof(T).Name,
                    method.DeclaringType.Name, method.Name));
                yield return () => { method.Invoke(null, null); };
            }
        }

        public static void ReloadActions()
        {
            onClearCache = GetActions<OnClearCache>().ToList();
            onDefsLoaded = GetActions<OnDefsLoaded>().ToList();
            onMapComponentsInitializing = GetActions<OnMapComponentsInitializing>().ToList();
            onTick = GetActions<OnTick>().ToList();
            onTickLong = GetActions<OnTickLong>().ToList();
        }

        static Main()
        {
            onStaticConstructors = GetActions<OnStaticConstructor>().ToList();
            for (var i = 0; i < onStaticConstructors.Count; i++) onStaticConstructors[i].Invoke();
        }

        public override void MapComponentsInitializing(Map map)
        {
            base.MapComponentsInitializing(map);

            for (var i = 0; i < onMapComponentsInitializing.Count; i++) onMapComponentsInitializing[i].Invoke();
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            {
                RocketPatcher.PatchAll();
                Finder.rocket.PatchAll();
            }
            for (var i = 0; i < onDefsLoaded.Count; i++) onDefsLoaded[i].Invoke();
        }

        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);

            if (currentTick % Finder.universalCacheAge != 0) return;

            for (var i = 0; i < onTick.Count; i++) onTick[i].Invoke();

            if (currentTick % (Finder.universalCacheAge * 5) != 0) return;

            for (var i = 0; i < onTickLong.Count; i++) onTickLong[i].Invoke();
        }

        public void ClearCache()
        {
            for (var i = 0; i < onClearCache.Count; i++) onClearCache[i].Invoke();
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class OnDefsLoaded : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class OnTickLong : Attribute
        {
        }


        [AttributeUsage(AttributeTargets.Method)]
        public class OnTick : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class OnMapComponentsInitializing : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class OnClearCache : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class OnStaticConstructor : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class OnScribe : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class OnInitialization : Attribute
        {
        }
    }
}