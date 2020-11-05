using System;
using HarmonyLib;
using RimWorld;

namespace RocketMan.Patches
{
    [HarmonyPatch(typeof(Pawn_TimetableTracker), nameof(Pawn_TimetableTracker.GetAssignment))]
    public static class Pawn_TimetableTracker_GetAssignment_Patch
    {
        static Exception Finalizer(Exception __exception, Pawn_TimetableTracker __instance, int hour, ref TimeAssignmentDef __result)
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
                finally
                {

                }
            }

            return null;
        }
    }
}
