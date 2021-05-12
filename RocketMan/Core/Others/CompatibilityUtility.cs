using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    public static class CompatibilityUtility
    {
        private static List<Pair<Type, bool>> _modLoadedCache = new List<Pair<Type, bool>>();

        public static bool IsModActive(Type modTypeHandler)
        {
            foreach (var storage in _modLoadedCache)
            {
                if (storage.First == modTypeHandler)
                    return storage.second;
            }
            bool isLoaded = (AccessTools.PropertyGetter(modTypeHandler, "Instance").Invoke(null, null) as ModHelper).IsLoaded();
            _modLoadedCache.Add(new Pair<Type, bool>(modTypeHandler, isLoaded));
            return isLoaded;
        }
    }
}
