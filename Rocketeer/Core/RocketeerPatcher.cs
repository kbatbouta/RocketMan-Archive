using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Rocketeer
{
    public class RocketeerPatcher
    {
        private static int currentReportId = 0;

        private static HarmonyMethod mDebugTranspiler = new HarmonyMethod(AccessTools.Method(typeof(RocketeerPatcher), nameof(RocketeerPatcher.Debug_Transpiler)));
        private static HarmonyMethod mDebugFinalizer = new HarmonyMethod(AccessTools.Method(typeof(RocketeerPatcher), nameof(RocketeerPatcher.Debug_Finalizer)));

        private static MethodBase mNotify_Call = AccessTools.Method(typeof(RocketeerPatcher), nameof(RocketeerPatcher.Notify_Call));
        private static MethodBase mNotify_CheckPoint = AccessTools.Method(typeof(RocketeerPatcher), nameof(RocketeerPatcher.Notify_CheckPoint));

        private static MethodBase mNotify_Finished = AccessTools.Method(typeof(RocketeerPatcher), nameof(RocketeerPatcher.Notify_Finished));
        private static MethodBase mNotify_Started = AccessTools.Method(typeof(RocketeerPatcher), nameof(RocketeerPatcher.Notify_Started));

        private static void Notify_Started(int reportId)
        {
            Tools.GetReport(reportId).OnStart();
        }

        private static void Notify_Call(int index, int reportId)
        {
            Tools.GetReport(reportId).OnCheckPoint(index);
        }

        private static void Notify_CheckPoint(int position, int reportId)
        {
            Tools.GetReport(reportId).OnCheckPoint(position);

        }

        private static void Notify_Finished(int reportId)
        {
            Tools.GetReport(reportId).OnFinished();
        }

        private static Exception Debug_Finalizer(Exception __exception)
        {
            return __exception;
        }

        private static IEnumerable<CodeInstruction> Debug_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            int index = 0;
            int counter = 0;
            Tools.GetReport(index).SaveInstructions(instructions);
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, Context.reportIdCounter) { labels = instruction.labels };
                    yield return new CodeInstruction(OpCodes.Call, mNotify_Finished);
                    instruction.labels = new List<Label>();
                }
                else
                {
                    if (counter % 3 == 0 && counter != 0)
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_I4, counter);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, Context.reportIdCounter);
                        yield return new CodeInstruction(OpCodes.Call, mNotify_CheckPoint);
                    }
                    if (instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Calli || instruction.opcode == OpCodes.Callvirt)
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_I4, index++);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, Context.reportIdCounter);
                        yield return new CodeInstruction(OpCodes.Call, mNotify_Call);
                    }
                    counter++;
                }
                yield return instruction;
            }
        }

        public RocketeerPatcher()
        {

        }
    }
}