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

    public class RocketeerPatchTracker
    {
        private readonly int patchId;
        private readonly MethodBase method;

        public struct RocketeerInstruction
        {
            public OpCode opCode;
            public object operand;
        }

        public int Id
        {
            get => patchId;
        }

        public MethodBase Method
        {
            get => method;
        }

        public int ExecutionRunsCounter
        {
            get => successCounter + errorCounter;
        }

        public int Resolution
        {
            get => resolution;
        }

        public bool flushImmediately = false;
        public int allocatedRuns = 128;

        private bool initialized = false;
        private bool expired = false;

        private int successCounter = 0;
        private int errorCounter = 0;
        private int resolution = 3;
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

        public RocketeerPatchTracker(MethodBase method, int patchId)
        {
            this.patchId = patchId;
            this.method = method;
            this.methodPath = method.GetDeclaredTypeMethodPath();
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
            if (Context.__MARCO > 0)
            {
                Context.__MARCO -= 1;
                Log.Warning($"ROCKETEER: PAULO:{Context.__MARCO}! from {method.GetDeclaredTypeMethodPath()}");
            }
            if (Context.__NUKE > 0)
            {
                Context.__NUKE -= 1;
                throw new Exception($"ROCKETEER: Boom! Total_runs:{ExecutionRunsCounter}");
            }
            if (expired)
            {
                Log.Warning($"ROCKETEER:[{method.GetDeclaredTypeMethodPath()}&{patchId}] An expired Rocketeer patch is need unpatching!");
                throw new Exception("ROCKETEER: This an expired rocketeer patch was caught active!");
            }
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
            currentInstructionIndex = callToPosition[callIndex];
            if (flushImmediately)
            {
                Flush(partial: true);
            }
        }

        public void OnCheckPoint(int sectionIndex)
        {
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
            allocatedRuns = Math.Max(allocatedRuns - 32, 0);
            errorCounter += 1;
            if (flushImmediately)
            {
                Flush(partial: false);
            }
            if (allocatedRuns <= 0)
            {
                Stop();
            }
        }

        public void Stop()
        {
            expired = true;
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
