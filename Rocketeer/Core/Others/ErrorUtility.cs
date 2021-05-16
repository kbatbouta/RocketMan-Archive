using System;
using System.Diagnostics;
using System.Reflection;
using RocketMan;
using Verse;

namespace Rocketeer
{
    public static class ErrorUtility
    {
        public static void Process(string text, StackTrace trace)
        {
            Log.Message($"ROCKETEER: Found {text.Substring(0, 10)}... in {trace.GetFrame(0).GetMethod().GetMethodPath()}");
            MethodBase method = trace.GetFrame(0).GetMethod();
        }
    }
}
