using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine.UI;

namespace Rocketeer
{
    public class RocketeerReport
    {
        private readonly int reportId;
        private readonly MethodBase method;

        public struct RocketeerInstruction
        {
            public string opCode;
            public string operand;
        }

        public int Id
        {
            get => reportId;
        }

        public MethodBase Method
        {
            get => method;
        }

        public RocketeerReport(MethodBase method)
        {
            this.reportId = Context.reportIdCounter++;
            this.method = method;
        }

        public void SaveInstructions(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                //var code = new RocketeerInstruction() { opCode =  };
            }
        }

        public void OnStart()
        {

        }

        public void OnCall(int index)
        {

        }

        public void OnCheckPoint(int position)
        {

        }

        public void OnFinished()
        {

        }

        public void OnError()
        {

        }
    }
}
