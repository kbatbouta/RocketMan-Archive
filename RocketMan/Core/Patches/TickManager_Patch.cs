using HarmonyLib;
using Verse;

namespace RocketMan.Patches
{
    [RocketPatch(typeof(TickManager), nameof(TickManager.TickRateMultiplier), MethodType.Getter, modsCompatiblityHandlers: new[] { typeof(MultiplayerHelper) })]
    public class TickManager_TickRateMultiplier_Patch
    {
        public static bool Prefix(ref float __result)
        {
            if (Finder.debug150MTPS && Finder.debug)
            {
                __result = 150f;
                return false;
            }
            return true;
        }
    }

    [RocketPatch(typeof(TickManager), nameof(TickManager.Notify_GeneratedPotentiallyHostileMap), modsCompatiblityHandlers: new[] { typeof(MultiplayerHelper) })]
    public class TickManager_Notify_GeneratedPotentiallyHostileMap_Patch
    {
        public static bool Prefix()
        {
            if (Prefs.DevMode) return false;
            else return true;
        }
    }
}