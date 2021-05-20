using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Proton
{
    [ProtonPatch(typeof(AlertsReadout), methodType: MethodType.Constructor)]
    public static class AlertsReadout_Constructor_Patch
    {
        public static void Postfix(AlertsReadout __instance) => AlertsManager.Initialize(__instance);
    }

    [ProtonPatch(typeof(AlertsReadout), nameof(AlertsReadout.AlertsReadoutUpdate))]
    public static class AlertsReadout_AlertsReadoutUpdate_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodBase mCheckAddOrRemoveAlert = AccessTools.Method(typeof(AlertsReadout), nameof(AlertsReadout.CheckAddOrRemoveAlert));
            bool finished = false;
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldc_I4_S && code.OperandIs(24))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlertsReadout_AlertsReadoutUpdate_Patch), nameof(AlertsReadout_AlertsReadoutUpdate_Patch.GetCount)));
                    continue;
                }
                if (!finished && code.opcode == OpCodes.Call && code.OperandIs(mCheckAddOrRemoveAlert))
                {
                    finished = true;
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlertsReadout_AlertsReadoutUpdate_Patch), nameof(AlertsReadout_AlertsReadoutUpdate_Patch.StartProfiling)));
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlertsReadout_AlertsReadoutUpdate_Patch), nameof(AlertsReadout_AlertsReadoutUpdate_Patch.StopProfiling)));
                    continue;
                }
                yield return code;
            }
        }

        private static int currentIndex = 0;
        private static Stopwatch stopwatch = new Stopwatch();

        private static void StartProfiling(AlertsReadout readout, int index)
        {
            currentIndex = index;
            stopwatch.Start();
        }

        private static void StopProfiling(AlertsReadout readout, int index)
        {
            if (currentIndex != index) return;
            stopwatch.Stop();
            float execuationTime = stopwatch.ElapsedTicks / Stopwatch.Frequency * 1000.0f;
            stopwatch.Reset();
        }

        private static int GetCount(AlertsReadout readout) => (int)Math.Max(readout.AllAlerts.Count * 0.75f, 24);
    }
}
