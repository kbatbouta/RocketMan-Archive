using System;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using RocketMan;
using UnityEngine;
using Verse;

namespace Rocketeer
{
    public static class Log_Error_Patch
    {
        [Main.OnInitialization]
        public static void OnDefsLoaded()
        {
            try
            {
                Log.Message("ROCKETEER: PATCHING STARTED!");
                Finder.harmony.Patch(AccessTools.Method(typeof(Log), "Error"), new HarmonyMethod(AccessTools.Method(typeof(Log_Error_Patch), "Prefix")));
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETEER: PATCHING FAILED! {er}");
            }
        }

        public static void Prefix(string text) => ErrorUtility.Process(text, new StackTrace(2, fNeedFileInfo: true));
    }
}
