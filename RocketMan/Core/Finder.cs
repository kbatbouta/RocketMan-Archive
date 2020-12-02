using HarmonyLib;
using Verse;

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
        public static bool timeDilation = false;
        public static bool timeDilationWorldPawns = true;
        public static bool flashDilatedPawns = false;
        public static bool alwaysDilating = false;
        public static bool translationCaching = true;
        public static bool thoughtsCaching = true;
        public static bool refreshGrid = false;
        public static bool enableGridRefresh = false;
        public static bool drawGlowerUpdates = false;
        public static bool statGearCachingEnabled = false;
        public static bool debug150MTPS = false;
        
        public static float learningRate = 0.05f;

        public static int universalCacheAge = 2500;
        public static int ageOfGetValueUnfinalizedCache = 0;

        public static byte[] statExpiry = new byte[ushort.MaxValue];
        public static bool[] dilatedDefs = new bool[ushort.MaxValue];

        public static readonly string HarmonyID = "NotooShabby.RocketMan";

        public static Harmony harmony = new Harmony(HarmonyID);
        public static RocketShip.RocketPatcher rocket = new RocketShip.RocketPatcher(HarmonyID);

        public static object locker = new object();
    }
}