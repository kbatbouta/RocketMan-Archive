using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace RocketMan
{
    public class RocketShip
    {
        [AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
        public class ConditionalHarmonyPatch : Attribute
        {
            public string targetMethod;
            public MethodType methodType;

            public Type targetType;
            public Type[] methodArguments;
            public Type[] genericsTypes;

            private bool found = false;

            private MethodInfo method;

            public Func<bool> check;

            public bool ShouldPatch => check();

            public ConditionalHarmonyPatch(string check, Type type, string methodName, MethodType methodType = MethodType.Normal, Type[] methodArguments = null, Type[] genericsTypes = null)
            {
                this.targetMethod = methodName;
                this.targetType = type;
                this.methodType = methodType;
                this.methodArguments = methodArguments;
                this.genericsTypes = genericsTypes;
                this.check = () => { return (bool)AccessTools.Method(check).Invoke(null, null); };
            }

            public MethodInfo GetMethodInfo()
            {
                if (found) return method;
                if (this.methodType == MethodType.Constructor)
                {
                    throw new NotImplementedException();
                }
                else if (this.methodType == MethodType.Normal)
                {
                    var m = AccessTools.Method(this.targetType, this.targetMethod, this.methodArguments, this.genericsTypes);
                    if (m != null) found = true;
                    method = m;
                    return m;
                }
                else if (this.methodType == MethodType.Getter)
                {
                    var m = AccessTools.PropertyGetter(this.targetType, this.targetMethod);
                    if (m != null) found = true;
                    method = m;
                    return m;
                }
                else if (this.methodType == MethodType.Setter)
                {
                    var m = AccessTools.PropertySetter(this.targetType, this.targetMethod);
                    if (m != null) found = true;
                    method = m;
                    return m;
                }
                throw new NotImplementedException();
            }
        }

        [AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
        public class SkipperPatch : Attribute
        {
            public string targetMethod;
            public MethodType methodType;

            public Type targetType;
            public Type[] methodArguments;
            public Type[] genericsTypes;

            private bool found = false;

            private MethodInfo method;

            public SkipperPatch(Type type, string methodName, MethodType methodType = MethodType.Normal, Type[] methodArguments = null, Type[] genericsTypes = null)
            {
                this.targetMethod = methodName;
                this.targetType = type;
                this.methodType = methodType;
                this.methodArguments = methodArguments;
                this.genericsTypes = genericsTypes;
            }

            public MethodInfo GetMethodInfo()
            {
                if (found) return method;
                if (this.methodType == MethodType.Constructor)
                {
                    throw new NotImplementedException();
                }
                else if (this.methodType == MethodType.Normal)
                {
                    var m = AccessTools.Method(this.targetType, this.targetMethod, this.methodArguments, this.genericsTypes);
                    if (m != null) found = true;
                    method = m;
                    return m;
                }
                else if (this.methodType == MethodType.Getter)
                {
                    var m = AccessTools.PropertyGetter(this.targetType, this.targetMethod);
                    if (m != null) found = true;
                    method = m;
                    return m;
                }
                else if (this.methodType == MethodType.Setter)
                {
                    var m = AccessTools.PropertySetter(this.targetType, this.targetMethod);
                    if (m != null) found = true;
                    method = m;
                    return m;
                }
                throw new NotImplementedException();
            }
        }

        public class RocketPatcher
        {
            public string id;
            public List<MethodInfo> patchedMethods = new List<MethodInfo>();
            public Dictionary<MethodInfo, Type> patches = new Dictionary<MethodInfo, Type>();

            private Harmony harmony;
            private static MethodInfo mTranspiler = AccessTools.Method("RocketPatcher:SkipperTranspiler");
            private static Type patchType;
            private static object locker = new object();

            public RocketPatcher(string id)
            {
                this.id = id;
                this.harmony = new Harmony(id + ".rocketpatch");
            }

            public void PatchAll()
            {
                var types = GetSkipperPatchTypes();
                foreach (Type t in types)
                {
                    var patchInfo = t.TryGetAttribute<SkipperPatch>();
                    this.Patch(patchInfo.GetMethodInfo(), t);
                }

                types = GetCompatibilityPatchTypes();
                foreach (Type t in types)
                {
                    var patchInfo = t.TryGetAttribute<ConditionalHarmonyPatch>();
                    if (patchInfo.check())
                    {
                        this.Patch(patchInfo.GetMethodInfo(),
                            AccessTools.Method(t, "Prefix"),
                            AccessTools.Method(t, "Postfix"),
                            AccessTools.Method(t, "Transpiler")
                            );
                    }
                    else
                    {
                        Log.Message(string.Format("ROCKETMAN: skipped target {0}", patchInfo.targetMethod));
                    }
                }
            }

            public void Patch(MethodInfo target, MethodInfo prefix, MethodInfo postfix, MethodInfo transpiler)
            {
                lock (locker)
                {
                    try
                    {
                        HarmonyMethod mprefix = null;
                        HarmonyMethod mpostfix = null;
                        HarmonyMethod mtranspiler = null;
                        if (prefix != null)
                        {
                            mprefix = new HarmonyMethod(prefix);
                        }
                        if (postfix != null)
                        {
                            mpostfix = new HarmonyMethod(postfix);
                        }
                        if (transpiler != null)
                        {
                            mtranspiler = new HarmonyMethod(transpiler);
                        }
                        this.harmony.Patch(target, mprefix, mpostfix, mtranspiler);
                        Log.Message(string.Format("ROCKETMAN: Patched {0} with prefix:{1}, postfix:{2}, transpiler:{3}", target, prefix, postfix, transpiler));
                    }
                    catch (Exception er)
                    {
                        Log.Error(string.Format("ROCKETMAN: error in patching {2} with {3} with error {0} at {1}", er.Message, er.StackTrace, target, patchType));
                    }
                }
            }

            public void Patch(MethodInfo target, Type patchType)
            {
                lock (locker)
                {
                    try
                    {
                        RocketPatcher.patchType = patchType;
                        this.harmony.Patch(target, transpiler: new HarmonyMethod(mTranspiler));
                        this.patchedMethods.Add(target);
                        this.patches.Add(target, patchType);
                        Log.Message(string.Format("ROCKETMAN: patched target {0}", target));
                    }
                    catch (Exception er)
                    {
                        Log.Error(string.Format("ROCKETMAN: error in patching {2} with {3} with error {0} at {1}", er.Message, er.StackTrace, target, patchType));
                    }
                }
            }

            public static IEnumerable<Type> GetCompatibilityPatchTypes()
            {
                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
                foreach (Type type in types)
                {
                    if (type.HasAttribute<ConditionalHarmonyPatch>())
                    {
                        Log.Message(string.Format("ROCKETMAN: found type {0} with compatibility patch attributes", type));
                        yield return type;
                    }
                }
            }

            public static IEnumerable<Type> GetSkipperPatchTypes()
            {
                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
                foreach (Type type in types)
                {
                    if (type.HasAttribute<SkipperPatch>())
                    {
                        Log.Message(string.Format("ROCKETMAN: found type {0} with skipper patch attributes", type));
                        yield return type;
                    }
                }
            }

            [UsedImplicitly]
            private static IEnumerable<CodeInstruction> SkipperTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
            {
                return SetupSkipping(instructions, generator, original,
                    AccessTools.Method(patchType, "Skipper"),
                    AccessTools.Method(patchType, "Setter"));
            }


            private static IEnumerable<CodeInstruction> SetupSkipping(IEnumerable<CodeInstruction> instructions,
                ILGenerator generator, MethodBase original, MethodBase skipper, MethodBase setter)
            {
                var codes = instructions.ToList();
                var returnType = (original as MethodInfo).ReturnType;

                LocalBuilder result = null;
                LocalBuilder state = null;

                if (returnType != typeof(void))
                {
                    result = generator.DeclareLocal(returnType);
                }

                if (skipper != null)
                {
                    if (TryGetStateType(skipper as MethodInfo, out var stateType))
                    {
                        state = generator.DeclareLocal(stateType);
                    }

                    var start = generator.DefineLabel();
                    if (returnType != typeof(void))
                    {
                        yield return new CodeInstruction(OpCodes.Ldloca_S, result.LocalIndex);
                    }
                    var extras = CallInside(original, skipper, state).ToList();
                    foreach (var extra in extras)
                        yield return extra;

                    yield return new CodeInstruction(OpCodes.Brtrue_S, start);
                    if (returnType != typeof(void))
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, result.LocalIndex);
                    }
                    yield return new CodeInstruction(OpCodes.Ret);

                    codes[0].labels.Add(start);
                }

                for (int i = 0; i < codes.Count; i++)
                {
                    var code = codes[i];
                    if (code.opcode == OpCodes.Ret && setter != null)
                    {
                        if (returnType != typeof(void))
                        {
                            yield return new CodeInstruction(OpCodes.Stloc_S, result.LocalIndex);
                            yield return new CodeInstruction(OpCodes.Ldloca_S, result.LocalIndex);
                        }
                        var extras = CallInside(original, setter, state).ToList();
                        extras[0].labels = code.labels;
                        foreach (var extra in extras)
                        {
                            yield return extra;
                        }

                        if (returnType != typeof(void))
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_S, result.LocalIndex);
                        }
                        yield return new CodeInstruction(OpCodes.Ret);
                    }
                    else
                    {
                        yield return code;
                    }
                }
            }

            private static bool TryGetStateType(MethodInfo skipper, out Type stateType)
            {
                if (skipper == null)
                {
                    stateType = typeof(void);
                    return false;
                }
                var mParameters = skipper.GetParameters();
                for (int i = 0; i < mParameters.Length; i++)
                    if (mParameters[i].Name.ToLower() == "__state")
                    {
                        stateType = mParameters[i].ParameterType;
                        return true;
                    }
                stateType = typeof(void);
                return false;
            }

            private static IEnumerable<CodeInstruction> CallInside(MethodBase parent, MethodBase method, LocalBuilder state = null)
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
                }

                for (int i = 0; i < mParameters.Length; i++)
                {
                    var methodParam = mParameters[i];
                    if (methodParam.Name == "__instance")
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        continue;
                    }

                    if (methodParam.Name == "__state" && state != null)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloca_S, state.LocalIndex);
                        continue;
                    }

                    for (int j = 0; j < pParameters.Length; j++)
                    {
                        var parentParam = pParameters[j];
                        if (methodParam.Name == parentParam.Name)
                        {
                            if (methodParam.ParameterType != parentParam.ParameterType && !methodParam.ParameterType.IsByRef)
                                throw new InvalidOperationException(
                                    string.Format("ROCKETMAN: error in patching:CallInside with method {0} with type mismatch {1}", parent.Name, methodParam.Name));
                            if (methodParam.ParameterType.IsByRef)
                                yield return new CodeInstruction(OpCodes.Ldarga_S, paramCounter);
                            else
                                yield return new CodeInstruction(OpCodes.Ldarg_S, paramCounter);
                            paramCounter++;
                        }
                    }

                }

                yield return new CodeInstruction(OpCodes.Call, method);
            }
        }
    }
}
