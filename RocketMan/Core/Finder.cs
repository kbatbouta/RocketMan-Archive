using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    [StaticConstructorOnStartup]
    public static class Finder
    {
        public static ModContentPack ModContentPack;

        public static bool WarmingUp
        {
            get => WarmUpMapComponent.settingsBeingStashed;
        }

        public static bool incompatibilityUnresolved = false;

        public static FieldInfo[] settingsFields;

        [Main.SettingsField(warmUpValue: false)]
        public static bool enabled = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool learning = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool labelCaching = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool alertThrottling = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool disableAllAlert = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool timeDilation = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool timeDilationCriticalHediffs = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool timeDilationWorldPawns = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool timeDilationVisitors = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool timeDilationCaravans = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool timeDilationColonyAnimals = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool translationCaching = true;

        [Main.SettingsField(warmUpValue: false)]
        public static bool thoughtsCaching = true;

        public static bool refreshGrid = false;

        public static bool enableGridRefresh = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool statGearCachingEnabled = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool corpsesRemovalEnabled = true;

        public static bool showWarmUpPopup = true;

        public static bool soyuzLoaded = false;

        public static bool protonLoaded = false;

        public static bool rocketeerLoaded = false;

        public static bool gagarinLoaded = false;

        public static int lastFrame;

        public static bool mainButtonToggle = true;

        public static float learningRate = 0.05f;

        public static int universalCacheAge = 2500;

        public static int ageOfGetValueUnfinalizedCache = 0;

        public static int ticksSinceStarted = 0;

        public static byte[] statExpiry = new byte[ushort.MaxValue];

        public static bool[] dilatedDefs = new bool[ushort.MaxValue];

        public static readonly string HarmonyID = "Krkr.RocketMan";

        public static RocketMod Mod;

        public static Harmony harmony = new Harmony(HarmonyID);

        public static RocketShip.SkipperPatcher rocket = new RocketShip.SkipperPatcher(HarmonyID);

        public static object locker = new object();
    }
}