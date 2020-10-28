using System;
using Verse;
using HarmonyLib;
using System.Collections.Generic;

namespace RocketMan
{
    [StaticConstructorOnStartup]
    public static class Finder
    {
        public static bool enabled = true;
        public static bool debug = false;
        public static bool statLogging = false;
        public static bool learning = true;
        public static bool labelCaching = true;
        public static bool translationCaching = true;
        public static bool thoughtsCaching = true;

        public static float learningRate = 0.005f;

        public static int universalCacheAge = 2500;
        public static int ageOfGetValueUnfinalizedCache = 0;

        public static byte[] statExpiry = new byte[ushort.MaxValue];

        public static readonly string HarmonyID = "NotooShabby.RocketMan";

        public static Harmony harmony = new Harmony(HarmonyID);
        public static RocketShip.RocketPatcher rocket = new RocketShip.RocketPatcher(HarmonyID);

        public static object locker = new object();
    }
}
