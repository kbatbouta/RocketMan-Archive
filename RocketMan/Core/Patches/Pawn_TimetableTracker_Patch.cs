using System;
using HarmonyLib;
using RimWorld;

namespace RocketMan.Patches
{
    [RocketPatch(typeof(Pawn_TimetableTracker), nameof(Pawn_TimetableTracker.GetAssignment))]
    public static class Pawn_TimetableTracker_GetAssignment_Patch
    {
        private static Exception Finalizer(Exception __exception, Pawn_TimetableTracker __instance, int hour,
            ref TimeAssignmentDef __result)
        {
            if (__exception != null)
            {
                try
                {
                    __result = TimeAssignmentDefOf.Anything;
                    __instance.SetAssignment(hour, TimeAssignmentDefOf.Anything);
                }
                catch
                {
                    return __exception;
                }
            }
            return null;
        }
    }
}