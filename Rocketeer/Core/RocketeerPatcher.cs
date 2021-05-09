using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private static RocketeerReport current;

        public static RocketeerReport Patch(MethodBase method)
        {
            RocketeerReport report = null;
            if (method.HasRocketeerPatch())
            {
                throw new InvalidOperationException($"ROCKETEER: target method is already patched! {method.IsValidTarget()}");
            }
            if (!method.IsValidTarget())
            {
                throw new InvalidOperationException($"ROCKETEER: target method is not valid! {method.IsValidTarget()}");
            }
            try
            {
                lock (Context.reportLocker)
                {
                    Context.reportIdCounter++;
                    report = new RocketeerReport(method, Context.reportIdCounter);
                    Context.reports[Context.reportIdCounter] = report;
                    Context.reportsByMethodPath[method.GetMethodPath()] = report;
                    current = report;
                    Harmony.DEBUG = true;
                    Finder.harmony.Patch(method, transpiler: mDebugTranspiler, finalizer: mDebugFinalizer);
                }
                Context.patchedMethods.Add(method.GetUniqueMethodIdentifier());
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETEER: Patching {method.GetMethodPath()} FAILED with error {er}");
                string methodId = method.GetUniqueMethodIdentifier();
                Context.patchedMethods.RemoveWhere(m => m == methodId);
            }
            finally
            {
                current = null;
            }
            return report;
        }

        public static void Unpatch(MethodBase method)
        {
            string methodId = method.GetUniqueMethodIdentifier();
            Finder.harmony.Unpatch(method, mDebugTranspiler.method);
            Finder.harmony.Unpatch(method, mDebugFinalizer.method);
            Context.patchedMethods.RemoveWhere(m => m == methodId);
        }

        private static void Notify_Started(int reportId)
        {
            Tools.GetReportById(reportId).OnStart();
        }

        private static void Notify_Call(int index, int reportId)
        {
            Tools.GetReportById(reportId).OnCall(index);
        }

        private static void Notify_CheckPoint(int checkPointIndex, int reportId)
        {
            Tools.GetReportById(reportId).OnCheckPoint(checkPointIndex);
        }

        private static void Notify_Finished(int reportId)
        {
            Tools.GetReportById(reportId).OnFinished();
        }

        private static Exception Debug_Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                var method = new StackTrace(__exception).GetFrame(0).GetMethod();
                Context.reportsByMethodPath[method.GetMethodPath()].OnError(__exception);
            }
            return __exception;
        }

        private static IEnumerable<CodeInstruction> Debug_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            int callCounter = 0;
            int instructionCounter = 0;
            int checkPointCounter = 0;
            yield return new CodeInstruction(OpCodes.Ldc_I4, Context.reportIdCounter);
            yield return new CodeInstruction(OpCodes.Call, mNotify_Started);
            if (current.Id != Context.reportIdCounter) throw new Exception("ROCKETEER: FATAL ERROR DURING PATCHING WHERE ID CHANGED!");
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, Context.reportIdCounter) { labels = instruction.labels };
                    yield return new CodeInstruction(OpCodes.Call, mNotify_Finished);
                    instruction.labels = new List<Label>();
                    if (instructionCounter + 1 < instructions.Count())
                    {
                        current.PushInstruction(instruction, BreakPointTypes.None);
                    }
                }
                else
                {
                    if (instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Calli || instruction.opcode == OpCodes.Callvirt)
                    {
                        current.PushInstruction(instruction, BreakPointTypes.Call);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, callCounter++);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, Context.reportIdCounter);
                        yield return new CodeInstruction(OpCodes.Call, mNotify_Call);
                    }
                    else if (instructionCounter % 3 == 0 && instructionCounter != 0)
                    {
                        current.PushInstruction(instruction, BreakPointTypes.CheckPoint);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, checkPointCounter++);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, Context.reportIdCounter);
                        yield return new CodeInstruction(OpCodes.Call, mNotify_CheckPoint);
                    }
                    else
                    {
                        current.PushInstruction(instruction, BreakPointTypes.None);
                    }
                }
                instructionCounter++;
                yield return instruction;
            }
            Log.Error($"{checkPointCounter}");
        }

        public RocketeerPatcher()
        {
        }
    }
}