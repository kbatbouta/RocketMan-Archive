using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Rocketeer
{
    public static class RocketeerPatchingUtility
    {
        private static int currentPatchId = 0;

        private static HarmonyMethod mDebugTranspiler = new HarmonyMethod(AccessTools.Method(typeof(RocketeerPatchingUtility), nameof(RocketeerPatchingUtility.Debug_Transpiler_newtemp)));

        private static MethodBase mNotify_Call = AccessTools.Method(typeof(RocketeerPatchingUtility), nameof(RocketeerPatchingUtility.Notify_Call));

        private static MethodBase mNotify_CheckPoint = AccessTools.Method(typeof(RocketeerPatchingUtility), nameof(RocketeerPatchingUtility.Notify_CheckPoint));

        private static MethodBase mNotify_Finished = AccessTools.Method(typeof(RocketeerPatchingUtility), nameof(RocketeerPatchingUtility.Notify_Finished));

        private static MethodBase mNotify_Started = AccessTools.Method(typeof(RocketeerPatchingUtility), nameof(RocketeerPatchingUtility.Notify_Started));

        private static MethodBase mNotify_Error = AccessTools.Method(typeof(RocketeerPatchingUtility), nameof(RocketeerPatchingUtility.Notify_Error));

        private static RocketeerPatchInfo current;


        public static MethodInfo PatchInternal(MethodInfo method)
        {
            current = new RocketeerPatchInfo(method, 0);
            Context.trackers[0] = current;
            MethodInfo methodInfo = Finder.harmony.Patch(
                        method,
                        transpiler: mDebugTranspiler);
            return methodInfo;
        }


        private static void Notify_Started(int trackerIndex)
        {
            // Context.trackers[trackerIndex]?.OnStart();
        }

        private static void Notify_Call(int index, int trackerIndex)
        {
            Context.trackers[trackerIndex]?.OnCall(index);
        }

        private static void Notify_CheckPoint(int checkPointIndex, int trackerIndex)
        {
            Context.trackers[trackerIndex]?.OnCheckPoint(checkPointIndex);
        }

        private static void Notify_Finished(int trackerIndex)
        {
            //Context.trackers[trackerIndex]?.OnFinished();
        }

        private static void Notify_Error(Exception exception, int trackerIndex)
        {
            // Context.trackers[trackerIndex]?.OnError(exception);
        }

        private static IEnumerable<CodeInstruction> Debug_Transpiler_newtemp(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = instructions.ToList();
            CodeInstruction instruction;
            Label exitPoint = generator.DefineLabel();
            Label returnPoint = generator.DefineLabel();
            yield return new CodeInstruction(OpCodes.Ldc_I4, current.Id);
            yield return new CodeInstruction(OpCodes.Call, mNotify_Started) { blocks = new List<ExceptionBlock>() { new ExceptionBlock(blockType: ExceptionBlockType.BeginExceptionBlock, typeof(object)) } };
            for (int i = 0; i < codes.Count; i++)
            {
                instruction = codes[i];
                if (instruction.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Leave_S, exitPoint) { labels = instruction.labels, blocks = instruction.blocks };
                    continue;
                }
                yield return instruction;
            }
            // Notify_Finished()
            yield return new CodeInstruction(OpCodes.Ldc_I4, current.Id) { labels = new List<Label>() { exitPoint } };
            yield return new CodeInstruction(OpCodes.Call, mNotify_Finished);
            // ------------
            // go to return and start of the try block
            yield return new CodeInstruction(OpCodes.Leave_S, returnPoint) { blocks = new List<ExceptionBlock>() { new ExceptionBlock(blockType: ExceptionBlockType.EndExceptionBlock, typeof(object)) } };
            //// ------------
            //// mNotify_Error() 
            //yield return new CodeInstruction(OpCodes.Ldc_I4, current.Id);
            //yield return new CodeInstruction(OpCodes.Call, mNotify_Error);
            //// ------------
            //// go to return #exit
            yield return new CodeInstruction(OpCodes.Leave_S, returnPoint) { labels = new List<Label>() { returnPoint } };

            yield return new CodeInstruction(OpCodes.Ret) { labels = new List<Label>() { returnPoint } };
        }

        private static IEnumerable<CodeInstruction> Debug_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            int callCounter = 0;
            int instructionCounter = 0;
            int checkPointCounter = 0;
            Label l1 = generator.DefineLabel();
            yield return new CodeInstruction(OpCodes.Ldc_I4, current.Id);
            yield return new CodeInstruction(OpCodes.Call, mNotify_Started);
            generator.BeginCatchBlock(typeof(Exception));
            if (current.Id != Context.patchIDCounter) throw new Exception("ROCKETEER: FATAL ERROR DURING PATCHING WHERE ID CHANGED!");
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, current.Id) { labels = instruction.labels };
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
                        yield return new CodeInstruction(OpCodes.Ldc_I4, current.Id);
                        yield return new CodeInstruction(OpCodes.Call, mNotify_Call);
                        continue;
                    }
                    else if (instructionCounter % 3 == 0 && instructionCounter != 0)
                    {
                        current.PushInstruction(instruction, BreakPointTypes.CheckPoint);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, checkPointCounter++);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, current.Id);
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