using System;
using HarmonyLib;

namespace RocketLite
{
    public static class Finder
    {
        public static string HarmonyID = "krk.rocketlite";

        public static Harmony harmony = new Harmony(HarmonyID);
    }
}
