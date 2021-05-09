using System;
using HarmonyLib;
using RocketMan;
using Verse;
using System.Runtime.InteropServices;

namespace Rocketeer
{
    public static class ContentTab_Soyuz_Patch
    {
        private static bool disabled = false;
        private static bool done = false;

        [Main.OnMapLoaded]
        public static void OnMapLoaded()
        {
            if (done || disabled)
            {
                if (done) Log.Warning("ROCKETEER: already patched ContentTabs in Soyuz");
                return;
            }
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                disabled = true;
                return;
            }
            done = true;
            RocketeerPatcher.Patch(AccessTools.Method("TabContent_Soyuz:DoExtras"));
            RocketeerPatcher.Patch(AccessTools.Method("TabContent_Soyuz:DoContent"));
        }
    }
}
