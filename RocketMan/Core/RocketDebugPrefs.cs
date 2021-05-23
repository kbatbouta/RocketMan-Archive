using System;
namespace RocketMan
{
    public static class RocketDebugPrefs
    {
        public static bool debug = false;

        public static bool statLogging = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool flashDilatedPawns = false;

        [Main.SettingsField(warmUpValue: false)]
        public static bool alwaysDilating = false;

        public static bool drawGlowerUpdates = false;

        public static bool logData = false;

        public static bool debug150MTPS = false;

        public static bool singleTickIncrement = false;

        public static int singleTickLeft = 0;
    }
}
