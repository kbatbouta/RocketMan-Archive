using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using HarmonyLib;
using RimWorld;
using RocketMan;
using Verse;

namespace Proton
{
    public static class AlertsReadout_Constructor_Patch
    {
        public static MethodBase mAlertsReadout = AccessTools.Constructor(typeof(AlertsReadout));
        public static MethodBase mPostfix = AccessTools.Method(typeof(AlertsReadout_Constructor_Patch), nameof(AlertsReadout_Constructor_Patch.Postfix));

        [Main.OnInitialization]
        public static void Patch()
        {
            Finder.harmony.Patch(mAlertsReadout, postfix: new HarmonyMethod((MethodInfo)mPostfix));
        }

        public static void Postfix(AlertsReadout __instance)
        {
            Context.alerts = __instance.AllAlerts.ToArray();
            Context.alertsSettings = new AlertSettings[Context.alerts.Length];
            int index = 0;
            Context.readoutInstance = __instance;
            foreach (Alert alert in Context.alerts)
            {
                string id = alert.GetType().Name;
                if (Context.typeIdToSettings.TryGetValue(alert.GetType().Name, out AlertSettings settings))
                {
                    Context.alertsSettings[index] = settings;
                }
                else
                {
                    settings = new AlertSettings() { typeId = id };
                    Context.typeIdToSettings[id] = settings;
                    Context.alertsSettings[index] = settings;
                }
                index++;
            }
        }
    }

    [ProtonPatch(typeof(AlertsReadout), nameof(AlertsReadout.AlertsReadoutUpdate))]
    public static class AlertsReadout_AlertsReadoutUpdate_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodBase mCheckAddOrRemoveAlert = AccessTools.Method(typeof(AlertsReadout), nameof(AlertsReadout.CheckAddOrRemoveAlert));
            Label l1 = generator.DefineLabel();
            Label l2 = generator.DefineLabel();
            Label l3 = generator.DefineLabel();
            bool finished = false;
            CodeInstruction[] codes = instructions.ToArray();
            for (int i = 0; i < codes.Length; i++)
            {
                CodeInstruction code = codes[i];
                if (code.opcode == OpCodes.Ldc_I4_S && code.OperandIs(24))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlertsReadout_AlertsReadoutUpdate_Patch), nameof(AlertsReadout_AlertsReadoutUpdate_Patch.GetCount)));
                    continue;
                }
                if (!finished && code.OperandIs(mCheckAddOrRemoveAlert))
                {
                    finished = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlertsReadout_AlertsReadoutUpdate_Patch), nameof(AlertsReadout_AlertsReadoutUpdate_Patch.ShouldUpdate)));

                    yield return new CodeInstruction(OpCodes.Brtrue_S, l2);

                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Br_S, l1);

                    yield return new CodeInstruction(OpCodes.Ldloc_0) { labels = new List<Label>() { l2 } };
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlertsReadout_AlertsReadoutUpdate_Patch), nameof(AlertsReadout_AlertsReadoutUpdate_Patch.StartProfiling)));

                    yield return code;

                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlertsReadout_AlertsReadoutUpdate_Patch), nameof(AlertsReadout_AlertsReadoutUpdate_Patch.StopProfiling)));

                    if (codes[i + 1].labels == null)
                    {
                        codes[i + 1].labels = new List<Label>();
                    }
                    codes[i + 1].labels.Add(l1);
                    continue;
                }
                yield return code;
            }
        }

        private static readonly Stopwatch stopwatch = new Stopwatch();

        private static bool ShouldUpdate(int index)
        {
            if (!Finder.enabled)
            {
                return true;
            }
            AlertSettings settings = Context.alertsSettings[index];
            if (settings == null)
            {
                return true;
            }
            return settings.enabled && settings.ShouldUpdate;
        }

        private static void StartProfiling(int index)
        {
            stopwatch.Restart();
        }

        private static void StopProfiling(int index)
        {
            if (!Finder.enabled)
            {
                return;
            }
            Context.alertsSettings[index]?.UpdatePerformanceMetrics((float)stopwatch.ElapsedTicks * 1000.0f / (float)Stopwatch.Frequency);
            stopwatch.Stop();
        }

        private static int GetCount(AlertsReadout readout) => (int)Math.Max(readout.AllAlerts.Count * 0.75f, 24);
    }
}
