using System;
using System.Collections.Generic;
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

    public class RocketeerReport
    {
        private readonly int reportId;
        private readonly MethodBase method;

        public struct RocketeerInstruction
        {
            public OpCode opCode;
            public object operand;
        }

        public struct RocketeerErrorReport
        {
            public string methodPath;
            public string[] stackTrace;
            public string message;
            public string type;
            public int lastInstructionIndex;
            public int lastSectionIndex;
            public int[] passes;
        }

        public int Id
        {
            get => reportId;
        }

        public MethodBase Method
        {
            get => method;
        }

        public int ExecutionRunsCounter
        {
            get => successCounter + errorCounter;
        }

        public bool flushImmediately = true;
        public int allocatedRuns = 1;

        private bool initialized = false;
        private bool disabled = false;

        private int successCounter = 0;
        private int errorCounter = 0;

        private int currentInstructionIndex = 0;
        private int currentSection = 0;
        private int t_currentSection = 0;

        private string methodPath;

        private readonly List<RocketeerInstruction> instructions = new List<RocketeerInstruction>();
        private readonly List<int> t_indexToSection = new List<int>();
        private readonly List<int> t_callToPosition = new List<int>();
        private readonly List<int> t_sectionsStartPosition = new List<int>();

        private int[] indexToSection;
        private int[] callToPosition;
        private int[] sectionsStartPosition;
        private int[] sectionsPasses;

        public RocketeerReport(MethodBase method, int reportId)
        {
            this.reportId = reportId;
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

        public void OnStart()
        {
            if (disabled)
                return;
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
            if (flushImmediately)
            {
                Flush(partial: true);
            }
        }

        public void OnCall(int callIndex)
        {
            if (disabled)
                return;
            currentInstructionIndex = callToPosition[callIndex];
            if (flushImmediately)
            {
                Flush(partial: true);
            }
        }

        public void OnCheckPoint(int sectionIndex)
        {
            if (disabled)
                return;
            currentInstructionIndex = sectionsStartPosition[sectionIndex];
            currentSection = sectionIndex;
            sectionsPasses[currentSection]++;
            if (flushImmediately)
            {
                Flush(partial: true);
            }
        }

        public void OnFinished()
        {
            if (disabled)
                return;
            allocatedRuns -= 1;
            successCounter++;
            if (flushImmediately)
            {
                Flush(partial: false);
            }
            if (allocatedRuns <= 0)
            {
                Stop();
            }
        }

        public void OnError(Exception exception)
        {
            if (disabled)
                return;
            allocatedRuns -= 1;
            errorCounter++;
            if (flushImmediately)
            {
                Flush(partial: false);
            }
            RocketeerErrorReport errorReport = new RocketeerErrorReport()
            {
                methodPath = method.GetMethodPath(),
                message = exception.Message,
                type = exception.GetType().ToString(),
                lastInstructionIndex = currentInstructionIndex,
                lastSectionIndex = currentSection,
                stackTrace = exception.GetStackTraceAsString(),
            };
            if (allocatedRuns <= 0)
            {
                Stop();
            }
        }

        public void Stop()
        {
            disabled = true;
            RocketeerPatcher.Unpatch(method);
        }

        public void Flush(bool partial = false)
        {
            if (partial)
            {
                for (int i = sectionsStartPosition[currentSection]; i < instructions.Count; i++)
                    Log.Message($"ROCKETEER:[{methodPath}] Reached instruction for the { sectionsPasses[currentSection] }th time {instructions[i].opCode}:{instructions[i].operand}");
                return;
            }
        }
    }
}
