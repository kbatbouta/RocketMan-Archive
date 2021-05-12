using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HugsLib;
using RocketMan.Tabs;
using Verse;

namespace RocketMan
{
    [StaticConstructorOnStartup]
    public class Main : ModBase
    {
        private int debugging = 0;

        private static List<Action> onClearCache;
        private static List<Action> onDefsLoaded;
        private static List<Action> onWorldLoaded;
        private static List<Action> onMapLoaded;
        private static List<Action> onMapComponentsInitializing;
        private static List<Action> onTick;
        private static List<Action> onTickLong;

        public static List<Action> onStaticConstructors;
        public static List<Action> onInitialization;
        public static List<Action> onScribe;

        public static List<Func<ITabContent>> yieldTabContent;

        public static List<Action> onDebugginEnabled;
        public static List<Action> onDebugginDisabled;

        public static void ReloadActions()
        {
            onClearCache = FunctionUtility.GetActions<OnClearCache>().ToList();
            onDefsLoaded = FunctionUtility.GetActions<OnDefsLoaded>().ToList();
            onWorldLoaded = FunctionUtility.GetActions<OnWorldLoaded>().ToList();
            onMapLoaded = FunctionUtility.GetActions<OnMapLoaded>().ToList();
            onMapComponentsInitializing = FunctionUtility.GetActions<OnMapComponentsInitializing>().ToList();
            onTick = FunctionUtility.GetActions<OnTick>().ToList();
            onDebugginEnabled = FunctionUtility.GetActions<OnDebugginEnabled>().ToList();
            onDebugginDisabled = FunctionUtility.GetActions<OnDebugginDisabled>().ToList();
            onTickLong = FunctionUtility.GetActions<OnTickLong>().ToList();
            yieldTabContent = FunctionUtility.GetFunctions<YieldTabContent, ITabContent>().ToList();
            onScribe = FunctionUtility.GetActions<Main.OnScribe>().ToList();
            onStaticConstructors = FunctionUtility.GetActions<Main.OnStaticConstructor>().ToList();
            onInitialization = FunctionUtility.GetActions<Main.OnInitialization>().ToList();
        }

        static Main()
        {
            onStaticConstructors = FunctionUtility.GetActions<OnStaticConstructor>().ToList();
            for (var i = 0; i < onStaticConstructors.Count; i++) onStaticConstructors[i].Invoke();
        }

        public override void MapLoaded(Map map)
        {
            base.MapLoaded(map);
            for (var i = 0; i < onMapLoaded.Count; i++) onMapLoaded[i].Invoke();
        }

        public override void WorldLoaded()
        {
            base.WorldLoaded();
            for (var i = 0; i < onWorldLoaded.Count; i++) onWorldLoaded[i].Invoke();
        }

        public override void MapComponentsInitializing(Map map)
        {
            base.MapComponentsInitializing(map);
            for (var i = 0; i < onMapComponentsInitializing.Count; i++) onMapComponentsInitializing[i].Invoke();
        }

        public override void DefsLoaded()
        {
            for (var i = 0; i < onDefsLoaded.Count; i++) onDefsLoaded[i].Invoke();
            base.DefsLoaded();
            {
                RocketPatcher.PatchAll();
                Finder.rocket.PatchAll();
            }
        }

        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);
            CheckDebugging();

            if (currentTick % Finder.universalCacheAge != 0) return;

            for (var i = 0; i < onTick.Count; i++) onTick[i].Invoke();

            if (currentTick % (Finder.universalCacheAge * 5) != 0) return;

            for (var i = 0; i < onTickLong.Count; i++) onTickLong[i].Invoke();
        }

        public void ClearCache()
        {
            for (var i = 0; i < onClearCache.Count; i++) onClearCache[i].Invoke();
        }

        private void CheckDebugging()
        {
            bool changed = false;
            switch (debugging)
            {
                case 0:
                    if (Finder.debug == true)
                        changed = true;
                    else return;
                    break;
                case 1:
                    if (Finder.debug == false)
                        return;
                    debugging = 2;
                    changed = true;
                    break;
                case 2:
                    if (Finder.debug == true)
                        return;
                    debugging = 1;
                    changed = true;
                    break;
            }
            if (!changed)
                return;
            if (debugging == 1)
            {
                for (var i = 0; i < onDebugginDisabled.Count; i++) onDebugginDisabled[i].Invoke();
            }
            else if (debugging == 2)
            {
                for (var i = 0; i < onDebugginEnabled.Count; i++) onDebugginEnabled[i].Invoke();
            }
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
        public class OnWorldLoaded : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class OnMapLoaded : Attribute
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

        [AttributeUsage(AttributeTargets.Method)]
        public class YieldTabContent : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class OnDebugginEnabled : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method)]
        public class OnDebugginDisabled : Attribute
        {
        }
    }
}