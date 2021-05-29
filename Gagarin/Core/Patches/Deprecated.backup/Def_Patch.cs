using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RocketMan;
using Verse;

namespace Gagarin
{
    // public static class Def_Patch
    // {
    //    [Main.OnInitialization]
    //    public static void Start()
    //    {
    //        new GagarinPatchInfo(typeof(Constructor_Patch)).Patch(Finder.harmony);
    //    }
    //
    //    [GagarinPatch]
    //    public static class Constructor_Patch
    //    {
    //        public static IEnumerable<MethodBase> TargetMethods()
    //        {
    //            HashSet<MethodBase> consturctors = new HashSet<MethodBase>();
    //            foreach (Type type in typeof(Def).AllSubclassesNonAbstract())
    //            {
    //                if (type.IsGenericType || type.IsSealed)
    //                {
    //                    continue;
    //                }
    //                ProcessDefType(type, consturctors);
    //            }
    //            static void ProcessDefType(Type type, HashSet<MethodBase> consturctors)
    //            {
    //                foreach (ConstructorInfo consturctorInfo in type.GetConstructors())
    //                {
    //                    if (false
    //                        || consturctorInfo.IsGenericMethod
    //                        || consturctorInfo.IsStatic
    //                        || consturctorInfo.DeclaringType != consturctorInfo.ReflectedType)
    //                    {
    //                        continue;
    //                    }
    //                    MethodBase consturctor = consturctorInfo as MethodBase;
    //                    if (consturctor != null && consturctor.IsValidTarget() && !consturctor.IsStatic)
    //                    {
    //                        consturctors.Add(consturctor);
    //                    }
    //                }
    //            }
    //            ProcessDefType(typeof(Def), consturctors);
    //            return consturctors;
    //        }
    //
    //        private static MethodBase mRegister = AccessTools.Method(typeof(Constructor_Patch), nameof(Constructor_Patch.Register));
    //
    //        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    //        {
    //            yield return new CodeInstruction(OpCodes.Ldarg_0);
    //            yield return new CodeInstruction(OpCodes.Castclass, typeof(Def));
    //            yield return new CodeInstruction(OpCodes.Call, mRegister);
    //            foreach (CodeInstruction code in instructions)
    //                yield return code;
    //        }
    //
    //        public static void Register(Def __instance)
    //        {
    //            Context.defs.Add(__instance);
    //        }
    //    }
    // }
}
