using System.Collections.Generic;
using System.Reflection;
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

        public static bool timeDilation = true;

        public static bool timeDilationCriticalHediffs = true;

        public static bool timeDilationWorldPawns = true;

        public static bool timeDilationVisitors = false;

        public static bool timeDilationCaravans = false;

        public static bool flashDilatedPawns = false;

        public static bool alwaysDilating = false;

        public static bool timeDilationColonyAnimals = true;

        public static bool translationCaching = true;

        public static bool thoughtsCaching = true;

        public static bool refreshGrid = false;

        public static bool enableGridRefresh = false;

        public static bool drawGlowerUpdates = false;

        public static bool statGearCachingEnabled = false;

        public static bool corpsesRemovalEnabled = true;

        public static bool logData = false;

        public static bool debug150MTPS = false;

        public static bool soyuzLoaded = false;

        public static bool protonLoaded = false;

        public static bool rocketeerLoaded = false;

        public static int lastFrame;

        public static float learningRate = 0.05f;

        public static int universalCacheAge = 2500;

        public static int ageOfGetValueUnfinalizedCache = 0;

        public static byte[] statExpiry = new byte[ushort.MaxValue];

        public static bool[] dilatedDefs = new bool[ushort.MaxValue];

        public static readonly string HarmonyID = "Krkr.RocketMan";

        public static RocketMod rocketMod;

        public static Harmony harmony = new Harmony(HarmonyID);

        public static RocketShip.SkipperPatcher rocket = new RocketShip.SkipperPatcher(HarmonyID);

        public static object locker = new object();

        public static readonly HashSet<Assembly> assemblies = new HashSet<Assembly>();

        public static IEnumerable<Assembly> RocketManAssemblies
        {
            get
            {
                Assembly mainAssembly = typeof(Finder).Assembly;
                if (!assemblies.Contains(mainAssembly))
                    assemblies.Add(mainAssembly);
                return assemblies;
            }
        }
    }
}