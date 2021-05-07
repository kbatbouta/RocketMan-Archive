using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.Experimental.XR;
using Verse;
using Verse.AI;

namespace RocketMan.Optimizations
{
    public class Pawn_Patch
    {
        private static bool suspended = false;
        private static bool offScreen = false;
        private static int instanceIDNumber;
        private static CellRect viewRect;

        private const int dilationConst = 4;

        [HarmonyPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.CostToPayThisTick))]
        public class Pawn_PathFollower_Patch
        {
            public static void Postfix(Pawn_PathFollower __instance, ref float __result)
            {
                if (offScreen && instanceIDNumber == __instance.pawn.thingIDNumber) __result = __result * dilationConst;
            }
        }

        [HarmonyPatch(typeof(CameraDriver), nameof(CameraDriver.Update))]
        public class CameraDriver_Update_Patch
        {
            public static void Prefix()
            {
                viewRect = Find.CameraDriver.CurrentViewRect;
            }
        }

        [HarmonyPatch(typeof(Thing), nameof(Thing.Suspended), MethodType.Getter)]
        public class Pawn_Suspended_Patch
        {
            public static void Postfix(Thing __instance, ref bool __result)
            {
                if (instanceIDNumber == __instance.thingIDNumber)
                {
                    suspended = __result;
                    if (offScreen) __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Tick))]
        public class Pawn_Tick_Patch
        {
            public static void Prefix(Pawn __instance)
            {
                if (Finder.enabled && Finder.timeDilation && __instance.factionInt == null && __instance.RaceProps.Animal)
                {
                    if (viewRect == null) viewRect = Find.CameraDriver.CurrentViewRect;
                    if (!viewRect.Contains(__instance.positionInt) && (__instance.thingIDNumber + GenTicks.TicksGame) % dilationConst != 0)
                    {
                        instanceIDNumber = __instance.thingIDNumber;
                        offScreen = true;
                    }
                    else
                    {
                        offScreen = false;
                    }
                }
                else
                {
                    instanceIDNumber = -1;
                    offScreen = false;
                }
            }

            public static void Postfix(Pawn __instance)
            {
                if (offScreen)
                {
                    TickExtras(__instance);
                }
            }

            private static void TickExtras(Pawn pawn)
            {
                if (!suspended)
                {
                    pawn.health.HealthTick();
                    if (!pawn.Dead)
                    {
                        pawn.mindState.MindStateTick();
                        pawn.carryTracker.CarryHandsTick();
                    }
                    pawn.ageTracker.AgeTick();
                    pawn.records.RecordsTick();
                }
            }
        }
    }
}
