using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RocketMan;
using Verse;

namespace RocketMan
{
    public enum PatchType
    {
        normal = 0,
        empty = 1
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RocketPatch : Attribute
    {
        public Type targetType;
        public string targetMethod;
        public Type[] parameters = null;
        public Type[] generics = null;
        public MethodType methodType;

        public readonly PatchType patchType;

        public RocketPatch()
        {
            this.patchType = PatchType.empty;
        }

        public RocketPatch(Type targetType, string targetMethod, MethodType methodType = MethodType.Normal, Type[] parameters = null, Type[] generics = null)
        {
            this.patchType = PatchType.normal;
            this.targetType = targetType;
            this.targetMethod = targetMethod;
            this.methodType = methodType;
            this.parameters = parameters;
            this.generics = generics;
        }
    }

    public class RocketPatchInfo
    {
        private RocketPatch attribute;
        private MethodBase[] targets;

        private MethodInfo prefix;
        private MethodInfo postfix;
        private MethodInfo transpiler;
        private MethodInfo finalizer;
        private MethodBase prepare;

        private PatchType patchType;

        public bool IsValid => attribute != null && targets.All(t => t != null);

        public RocketPatchInfo(Type type)
        {
            attribute = type.TryGetAttribute<RocketPatch>();
            patchType = attribute.patchType;
            try
            {
                if (patchType == PatchType.normal)
                {
                    if (attribute.methodType == MethodType.Getter)
                        targets = new MethodBase[1]
                            {AccessTools.PropertyGetter(attribute.targetType, attribute.targetMethod)};
                    else if (attribute.methodType == MethodType.Setter)
                        targets = new MethodBase[1]
                            {AccessTools.PropertySetter(attribute.targetType, attribute.targetMethod)};
                    else if (attribute.methodType == MethodType.Normal)
                        targets = new MethodBase[1]
                        {
                            AccessTools.Method(attribute.targetType, attribute.targetMethod, attribute.parameters,
                                attribute.generics)
                        };
                    else throw new NotImplementedException();
                }
                else if (patchType == PatchType.empty)
                {

                    targets = (type.GetMethod("TargetMethods").Invoke(null, null) as IEnumerable<MethodBase>).ToArray();
                }
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETMAN: target type {type.Name}:{er}");
                throw new Exception();
            }

            prepare = type.GetMethod("Prepare");
            prefix = type.GetMethod("Prefix");
            postfix = type.GetMethod("Postfix");
            transpiler = type.GetMethod("Transpiler");
            finalizer = type.GetMethod("Finalizer");
        }

        public void Patch(Harmony harmony)
        {
            if (prepare != null && !((bool)prepare.Invoke(null, null)))
            {
                if (Finder.debug) Log.Message($"ROCKETMAN: Prepare failed for {attribute.targetType.Name ?? null}:{attribute.targetMethod ?? null}");
                return;
            }

            foreach (var target in targets.ToHashSet())
            {
                if (!target.IsValidTarget())
                {
                    if (Finder.debug) Log.Warning($"ROCKETMAN:[NOTANERROR] patching {target?.DeclaringType?.Name}:{target} is not possible! Patch attempt skipped");
                    continue;
                }
                try
                {
                    harmony.Patch(target,
                        prefix: prefix != null ? new HarmonyMethod(prefix) : null,
                        postfix: postfix != null ? new HarmonyMethod(postfix) : null,
                        transpiler: transpiler != null ? new HarmonyMethod(transpiler) : null,
                        finalizer: finalizer != null ? new HarmonyMethod(finalizer) : null);
                }
                catch (Exception er)
                {
                    Log.Warning($"ROCKETMAN: patching {target.DeclaringType.Name}:{target} is not possible! {er}");
                }
            }
        }
    }

    [StaticConstructorOnStartup]
    public class RocketPatcher
    {
        public static RocketPatchInfo[] patches = null;

        public static void PatchAll()
        {
            foreach (var patch in patches)
                patch.Patch(Finder.harmony);
            if (Finder.debug) Log.Message($"ROCKETMAN: Patching finished");
        }

        static RocketPatcher()
        {
            var flaggedTypes = GetSoyuzPatches();
            var patchList = new List<RocketPatchInfo>();
            foreach (var type in flaggedTypes)
            {
                var patch = new RocketPatchInfo(type);
                patchList.Add(patch);
                if (Finder.debug) Log.Message($"ROCKETMAN: Found patch in {type} and is {(patch.IsValid ? "valid" : "invalid") }");
            }
            patches = patchList.Where(p => p.IsValid).ToArray();
        }

        private static IEnumerable<Type> GetSoyuzPatches()
        {
            return typeof(RocketPatcher).Assembly.GetTypes().Where(
                t => t.HasAttribute<RocketPatch>()
                );
        }
    }
}