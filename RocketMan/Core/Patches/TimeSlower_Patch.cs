using HarmonyLib;
using Verse;

namespace RocketMan.Patches
{
    [RocketPatch(typeof(TimeSlower), nameof(TimeSlower.SignalForceNormalSpeed), modsCompatiblityHandlers: new[] { typeof(MultiplayerHelper) })]
    public class TimeSlower_SignalForceNormalSpeed_Patch
    {
        public static bool Prefix() => !Prefs.DevMode;
    }

    [RocketPatch(typeof(TimeSlower), nameof(TimeSlower.SignalForceNormalSpeedShort), modsCompatiblityHandlers: new[] { typeof(MultiplayerHelper) })]
    public class TimeSlower_SignalForceNormalSpeedShort_Patch
    {
        public static bool Prefix() => !Prefs.DevMode;
    }
}