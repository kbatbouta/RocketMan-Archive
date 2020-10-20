using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace RocketMan
{
    public static class HarmonyUtility
    {
        public const string HarmonyID = "NotooShabby.RocketMan";

        public static Harmony Instance = new Harmony(HarmonyID);
    }
}