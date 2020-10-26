using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RocketMan
{
    public class RocketShip
    {
        public static IEnumerable<CodeInstruction> SkipperPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            return RocketShip.SetupSkipping(instructions, generator, original,
                    AccessTools.Method(GetStaticType(), "SkipFix"),
                    AccessTools.Method(GetStaticType(), "SetFix"));
        }

        private static Type GetStaticType()
        {
            var stack = new System.Diagnostics.StackTrace();

            if (stack.FrameCount < 2)
                return null;

            return (stack.GetFrame(2).GetMethod() as System.Reflection.MethodInfo).DeclaringType;
        }


        private static IEnumerable<CodeInstruction> SetupSkipping(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator, MethodBase original, MethodBase skipfix, MethodBase setfix)
        {
            var codes = instructions.ToList();
            var returnType = (original as MethodInfo).ReturnType;

            LocalBuilder result = generator.DeclareLocal(returnType);
            LocalBuilder execute = generator.DeclareLocal(typeof(bool));

            if (skipfix != null)
            {
                var start = generator.DefineLabel();

                yield return new CodeInstruction(OpCodes.Ldloca_S, result.LocalIndex);

                var extras = CallInside(original, skipfix).ToList();
                foreach (var extra in extras)
                    yield return extra;

                yield return new CodeInstruction(OpCodes.Stloc_S, execute.LocalIndex);
                yield return new CodeInstruction(OpCodes.Ldloc_S, execute.LocalIndex);
                yield return new CodeInstruction(OpCodes.Brtrue_S, start);
                yield return new CodeInstruction(OpCodes.Ldloc_S, result.LocalIndex);
                yield return new CodeInstruction(OpCodes.Ret);

                codes[0].labels.Add(start);
            }

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Ret && setfix != null)
                {
                    yield return new CodeInstruction(OpCodes.Stloc_S, result.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, result.LocalIndex);
                    var extras = CallInside(original, setfix).ToList();
                    extras[0].labels = code.labels;
                    foreach (var extra in extras)
                    {
                        yield return extra;
                    }
                    yield return new CodeInstruction(OpCodes.Ldloc_S, result.LocalIndex);
                    yield return new CodeInstruction(OpCodes.Ret);
                }
                else
                {
                    yield return code;
                }
            }
        }

        private static IEnumerable<CodeInstruction> CallInside(MethodBase parent, MethodBase method)
        {
            if (!method.IsStatic)
                throw new InvalidOperationException(
                    string.Format("ROCKETMAN: can't use non static method {0} in a patch:CallInside", parent.Name));
            var mParameters = method.GetParameters();
            var pParameters = parent.GetParameters();

            var paramCounter = 0;
            if (!parent.IsStatic)
            {
                paramCounter += 1;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
            }

            for (int i = 0; i < mParameters.Length; i++)
            {
                var methodParam = mParameters[i];
                for (int j = 0; j < pParameters.Length; j++)
                {
                    var parentParam = pParameters[j];
                    if (methodParam.Name == parentParam.Name)
                    {
                        if (methodParam.ParameterType != parentParam.ParameterType)
                            throw new InvalidOperationException(
                                string.Format("ROCKETMAN: error in patching:CallInside with method {0} with type mismatch {1}", parent.Name, methodParam.Name));
                        yield return new CodeInstruction(OpCodes.Ldarg_S, paramCounter);
                        paramCounter++;
                    }
                }
            }

            yield return new CodeInstruction(OpCodes.Call, method);
        }
    }
}
