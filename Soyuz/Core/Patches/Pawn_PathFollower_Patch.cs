using System.Runtime.Remoting.Messaging;
using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Soyuz.Patches
{
    public static class Pawn_PathFollower_Patch
    {
        private static float remaining = 0f;
        private static Pawn curPawn;
        
        [SoyuzPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.CostToPayThisTick))]
        public class Pawn_PathFollower_CostToPayThisTick_Patch
        {
            public static void Postfix(Pawn_PathFollower __instance, ref float __result)
            {
                remaining = 0f;
                if (true
                    && __instance.pawn.IsValidWildlifeOrWorldPawn()
                    && __instance.pawn.IsSkippingTicks())
                {
                    curPawn = __instance.pawn;
                    var modified = __result * __instance.pawn.GetDeltaT();
                    var cost = __instance.nextCellCostLeft;
                    if (modified > cost)
                    {
                        remaining = modified - cost;
                        __result = cost;
                    }
                    else __result = modified;
                }
            } 
        }

        [SoyuzPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.SetupMoveIntoNextCell))]
        public class Pawn_PathFollower_SetupMoveIntoNextCell_Patch
        {
            public static void Postfix(Pawn_PathFollower __instance)
            {
                var pawn = __instance.pawn;
                if (pawn == curPawn)
                {
                    __instance.nextCellCostLeft -= remaining;
                    __instance.nextCellCostTotal -= remaining;
                    curPawn = null;
                }
            }   
        }
    }
}