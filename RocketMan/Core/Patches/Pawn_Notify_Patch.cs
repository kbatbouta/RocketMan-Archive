using HarmonyLib;
using RimWorld;
using Verse;

namespace RocketMan.Patches
{
    [HarmonyPatch]
    public static class Pawn_Notify_Dirty
    {
        [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelAdded))]
        [HarmonyPostfix]
        public static void Notify_ApparelAdded_Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            __instance.pawn.Notify_Dirty();
        }

        [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_ApparelRemoved))]
        [HarmonyPostfix]
        public static void Notify_ApparelRemoved_Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            __instance.pawn.Notify_Dirty();
        }

        [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.Notify_EquipmentAdded))]
        [HarmonyPostfix]
        public static void Notify_EquipmentAdded_Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            __instance.pawn.Notify_Dirty();
        }

        [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.Notify_EquipmentAdded))]
        [HarmonyPostfix]
        public static void Notify_EquipmentRemoved_Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            __instance.pawn.Notify_Dirty();
        }

        [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear))]
        [HarmonyPostfix]
        public static void Wear_Postfix(Pawn_ApparelTracker __instance)
        {
            __instance.pawn.Notify_Dirty();
        }

        [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Remove))]
        [HarmonyPostfix]
        public static void Remove_Postfix(Pawn_ApparelTracker __instance)
        {
            __instance.pawn.Notify_Dirty();
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Destroy))]
        [HarmonyPostfix]
        public static void Destroy_Postfix(Pawn __instance)
        {
            __instance.Notify_Dirty();
        }

        [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Notify_LostBodyPart))]
        [HarmonyPostfix]
        public static void Notify_LostBodyPart_Postfix(Pawn_ApparelTracker __instance)
        {
            __instance.pawn.Notify_Dirty();
        }

        [HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.ApparelChanged))]
        [HarmonyPostfix]
        public static void Notify_ApparelChanged_Postfix(Pawn_ApparelTracker __instance)
        {
            __instance.pawn.Notify_Dirty();
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Notify_BulletImpactNearby))]
        [HarmonyPostfix]
        public static void Notify_BulletImpactNearby_Postfix(Pawn __instance)
        {
            __instance.Notify_Dirty();
        }

        [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.Notify_HediffChanged))]
        [HarmonyPostfix]
        public static void Notify_HediffChanged_Postfix(Pawn_HealthTracker __instance)
        {
            __instance.pawn.Notify_Dirty();
        }

        [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.Notify_UsedVerb))]
        [HarmonyPostfix]
        public static void Notify_UsedVerb_Postfix(Pawn_HealthTracker __instance)
        {
            __instance.pawn.Notify_Dirty();
        }
    }
}