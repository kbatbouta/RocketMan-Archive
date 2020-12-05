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
        private readonly List<Action> onClearCache = GetActions<OnClearCache>().ToList();
        private readonly List<Action> onDefsLoaded = GetActions<OnDefsLoaded>().ToList();
        private readonly List<Action> onEarlyInitialize = GetActions<OnEarlyInitialize>().ToList();

        private readonly List<Action> onMapComponentsInitializing = GetActions<OnMapComponentsInitializing>().ToList();
        private readonly List<Action> onTick = GetActions<OnTick>().ToList();
        private readonly List<Action> onTickLong = GetActions<OnTickLong>().ToList();

        private static List<Action> onStaticConstructors;

        public static IEnumerable<Action> GetActions<T>() where T : Attribute
        {
            foreach (var method in AppDomain.CurrentDomain.GetAssemblies()
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

        public override void EarlyInitialize()
        {
            base.EarlyInitialize();

            for (var i = 0; i < onEarlyInitialize.Count; i++) onEarlyInitialize[i].Invoke();
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
        public class OnEarlyInitialize : Attribute
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
    }
}