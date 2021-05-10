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
    public static class RocketeerPatcher
    {
        private static int currentPatchId = 0;
        private static readonly object _patchPatchingLocker = new object();
        private static readonly object _patchLocker = new object();

        private static HarmonyMethod mDebugTranspiler = new HarmonyMethod(AccessTools.Method(typeof(RocketeerPatcher), nameof(RocketeerPatcher.Debug_Transpiler)));
        private static HarmonyMethod mDebugFinalizer = new HarmonyMethod(AccessTools.Method(typeof(RocketeerPatcher), nameof(RocketeerPatcher.Debug_Finalizer)));

        private static MethodBase mNotify_Call = AccessTools.Method(typeof(RocketeerPatcher), nameof(RocketeerPatcher.Notify_Call));
        private static MethodBase mNotify_CheckPoint = AccessTools.Method(typeof(RocketeerPatcher), nameof(RocketeerPatcher.Notify_CheckPoint));

        private static MethodBase mNotify_Finished = AccessTools.Method(typeof(RocketeerPatcher), nameof(RocketeerPatcher.Notify_Finished));
        private static MethodBase mNotify_Started = AccessTools.Method(typeof(RocketeerPatcher), nameof(RocketeerPatcher.Notify_Started));

        private static RocketeerPatchTracker current;

        public static RocketeerPatchTracker Patch(MethodBase method)
        {
            RocketeerPatchTracker tracker = null;
            if (method.IsPatched())
            {
                throw new InvalidOperationException($"ROCKETEER: target method is already patched! {method.IsValidTarget()}");
            }
            if (!method.IsValidTarget())
            {
                throw new InvalidOperationException($"ROCKETEER: target method is not valid! {method.IsValidTarget()}");
            }
            try
            {
                lock (_patchPatchingLocker)
                {
                    Context.patchIDCounter++;
                    tracker = new RocketeerPatchTracker(method, Context.patchIDCounter);
                    if (Context.patchIDCounter >= Context.patches.Length)
                    {
                        ExtentedTrackersCacheCapacity();
                    }
                    Context.patches[Context.patchIDCounter] = tracker;
                    Context.patchByUniqueIdentifier[method.GetUniqueMethodIdentifier()] = tracker;
                    current = tracker;
                    Harmony.DEBUG = true;
                    Finder.harmony.Patch(method, transpiler: mDebugTranspiler, finalizer: mDebugFinalizer);
                }
                Context.patchedMethods.Add(method.GetUniqueMethodIdentifier());
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETEER: Patching {method.GetDeclaredTypeMethodPath()} FAILED with error {er}");
                string methodId = method.GetUniqueMethodIdentifier();
                Context.patchedMethods.RemoveWhere(m => m == methodId);
            }
            finally
            {
                current = null;
            }
            return tracker;
        }

        public static void Unpatch(MethodBase method)
        {
            string methodId = method.GetUniqueMethodIdentifier();
            RocketeerPatchTracker tracker = Context.patchByUniqueIdentifier[methodId];
            Context.patches[tracker.Id] = null;
            Context.patchByUniqueIdentifier.Remove(methodId);
            Context.patchedMethods.RemoveWhere(m => m == methodId);
            Finder.harmony.Unpatch(method, mDebugTranspiler.method);
            Finder.harmony.Unpatch(method, mDebugFinalizer.method);
        }

        private static void ExtentedTrackersCacheCapacity()
        {
            RocketeerPatchTracker[] temp = new RocketeerPatchTracker[Context.patches.Length * 2 + 100];
            for (int i = 0; i < Context.patches.Length; i++)
            {
                temp[i] = Context.patches[i];
                Context.patches[i] = null;
            }
            Context.patches = temp;
        }


        private static void Notify_Started(int patchIndex)
        {
            Context.patches[patchIndex]?.OnStart();
        }

        private static void Notify_Call(int index, int patchIndex)
        {
            Context.patches[patchIndex]?.OnCall(index);
        }

        private static void Notify_CheckPoint(int checkPointIndex, int patchIndex)
        {
            Context.patches[patchIndex]?.OnCheckPoint(checkPointIndex);
        }

        private static void Notify_Finished(int patchIndex)
        {
            Context.patches[patchIndex]?.OnFinished();
        }

        private static Exception Debug_Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                var method = new StackTrace(__exception).GetFrame(0).GetMethod();
                Context.patchByUniqueIdentifier[method.GetUniqueMethodIdentifier()].OnError(__exception);
            }
            return __exception;
        }

        private static IEnumerable<CodeInstruction> Debug_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            int callCounter = 0;
            int instructionCounter = 0;
            int checkPointCounter = 0;

            yield return new CodeInstruction(OpCodes.Ldc_I4, Context.patchIDCounter);
            yield return new CodeInstruction(OpCodes.Call, mNotify_Started);
            if (current.Id != Context.patchIDCounter) throw new Exception("ROCKETEER: FATAL ERROR DURING PATCHING WHERE ID CHANGED!");
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, Context.patchIDCounter) { labels = instruction.labels };
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
                        instructionCounter++;
                        yield return instruction;
                        current.PushInstruction(instruction, BreakPointTypes.Call);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, callCounter++);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, Context.patchIDCounter);
                        yield return new CodeInstruction(OpCodes.Call, mNotify_Call);
                        continue;
                    }
                    else if (instructionCounter % current.Resolution == 0 && instructionCounter != 0)
                    {
                        current.PushInstruction(instruction, BreakPointTypes.CheckPoint);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, checkPointCounter++);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, Context.patchIDCounter);
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
        }
    }
}