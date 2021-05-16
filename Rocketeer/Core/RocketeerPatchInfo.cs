using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RocketMan;
using UnityEngine.UI;
using Verse;

namespace Rocketeer
{
    public enum BreakPointTypes
    {
        CheckPoint = 0,
        Call = 1,
        None = 2
    }

    public class RocketeerPatchInfo
    {
        private readonly int trackerId;
        private readonly MethodBase method;

        public struct RocketeerInstruction
        {
            public OpCode opCode;
            public object operand;
        }

        public int Id
        {
            get => trackerId;
        }

        public MethodBase Method
        {
            get => patchedMethodInfo != null ? patchedMethodInfo : patchedMethodInfo;
        }

        public string MethodPath
        {
            get => patchedMethodInfo != null ? patchedMethodInfo.GetMethodPath() : methodPath;
        }

        private bool initialized = false;
        private bool expired = false;
        private bool patched = false;
        private bool executing = false;

        public int successCounter = 0;
        public int errorCounter = 0;

        private int resolution = 3;
        private int currentInstructionIndex = 0;
        private int currentSection = 0;
        private string methodPath;

        private int t_currentSection = 0;
        private readonly List<RocketeerInstruction> instructions = new List<RocketeerInstruction>();
        private readonly List<int> t_indexToSection = new List<int>();
        private readonly List<int> t_callToPosition = new List<int>();
        private readonly List<int> t_sectionsStartPosition = new List<int>();

        private int[] indexToSection;
        private int[] callToPosition;
        private int[] sectionsStartPosition;
        private int[] sectionsPasses;

        private MethodInfo patchedMethodInfo;

        public RocketeerPatchInfo(MethodBase method, int trackerId)
        {
            this.trackerId = trackerId;
            this.method = method;
            this.methodPath = method.GetMethodPath();
        }

        public void PushInstruction(CodeInstruction instruction, BreakPointTypes pointTypes)
        {
            switch (pointTypes)
            {
                case BreakPointTypes.Call:
                    t_callToPosition.Add(instructions.Count);
                    break;
                case BreakPointTypes.CheckPoint:
                    t_currentSection = t_sectionsStartPosition.Count;
                    t_sectionsStartPosition.Add(instructions.Count);
                    break;
            }
            t_indexToSection.Add(t_currentSection);
            instructions.Add(new RocketeerInstruction()
            {
                opCode = instruction.opcode,
                operand = instruction.operand
            });
        }

        public void Notify_Patched(MethodInfo patchedMethodInfo)
        {
            this.patchedMethodInfo = patchedMethodInfo;
            this.patched = true;
        }

        public void OnStart()
        {
            if (Context.__MARCO > 0)
            {
                Context.__MARCO -= 1;
                Log.Warning($"ROCKETEER: PAULO:{Context.__MARCO}! from {Method.GetMethodPath()}");
            }
            if (executing)
            {
                Log.Error("Interupted!");
            }
            if (expired || !patched)
            {
                Log.Warning($"ROCKETEER:[{Method.GetMethodPath()}&{trackerId}] An expired Rocketeer patch is need unpatching!");
                throw new Exception("ROCKETEER: This an expired rocketeer patch was caught active!");
            }
            executing = true;
            currentSection = 0;
            currentInstructionIndex = 0;
            if (initialized)
            {
                for (int i = 0; i < sectionsStartPosition.Length; i++)
                    sectionsPasses[i] = 0;
                return;
            }
            indexToSection = t_indexToSection.ToArray();
            callToPosition = t_callToPosition.ToArray();
            sectionsStartPosition = t_sectionsStartPosition.ToArray();
            initialized = true;
            sectionsPasses = new int[sectionsStartPosition.Length];
            for (int i = 0; i < sectionsStartPosition.Length; i++)
                sectionsPasses[i] = 0;
        }

        public void OnCall(int callIndex)
        {
            executing = true;
            currentInstructionIndex = callToPosition[callIndex];
        }

        public void OnCheckPoint(int sectionIndex)
        {
            executing = true;
            currentInstructionIndex = sectionsStartPosition[sectionIndex];
            currentSection = sectionIndex;
            sectionsPasses[currentSection]++;
            // ----------------
            // TODO remove this
            if (Context.__NUKE > 0 && Rand.Chance(0.05f))
            {
                Context.__NUKE = Math.Max(Context.__NUKE - 1, 0);
                throw new Exception("BOOM!");
            }
        }

        public void OnFinished()
        {
            successCounter++;
            executing = false;
        }

        public void OnError(Exception exception)
        {
            errorCounter += 1;
            ProcessException(exception);
            executing = false;
        }

        public void Stop()
        {
            expired = true;
        }

        private void ProcessException(Exception exception)
        {
            string report = $"ROCETEER:<color=red>[{MethodPath}:ERROR]</color> An exception occured in {Method.Name}\n" +
                $"<color=red>Exception type:</color> {exception.GetType()}\n";
            string[] trace = exception.GetStackTraceAsString();
            foreach (var t in trace)
                report = $"{report}\n{trace}";
            report += "\n<color=red>excuted IL instructions</color>\n" +
                $"<color=red>Execution ended at {currentInstructionIndex}</color>\n" +
                $"INDEX:[TIMES PASSED]\tOpCode\tOprand\n";
            for (int i = 0; i < currentInstructionIndex; i++)
            {
                report = $"{report}\n" +
                    $"{currentInstructionIndex}\t:[{sectionsPasses[indexToSection[i]]}] {instructions[i].opCode}\t{instructions[i].operand}";
            }
            report = $"{report}\n<color=red>Exception message:</color>\n{exception.Message}";
            Log.Message(report);
            Log.Message("ROCKETEER: Error report generated!");
            // TODO fix this
            // this.Stop();
        }
    }
}
