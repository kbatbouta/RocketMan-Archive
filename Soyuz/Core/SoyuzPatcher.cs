using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Soyuz
{
    public enum PatchType
    {
        normal = 0,
        empty = 1
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SoyuzPatch : Attribute
    {
        public Type targetType;
        public string targetMethod;
        public Type[] parameters = null;
        public Type[] generics = null;
        public MethodType methodType;

        public readonly PatchType patchType;

        public SoyuzPatch()
        {
            this.patchType = PatchType.empty;
        }

        public SoyuzPatch(Type targetType, string targetMethod, MethodType methodType = MethodType.Normal, Type[] parameters = null, Type[] generics = null)
        {
            this.patchType = PatchType.normal;
            this.targetType = targetType;
            this.targetMethod = targetMethod;
            this.methodType = methodType;
            this.parameters = parameters;
            this.generics = generics;
        }
    }

    public class SoyuzPatchInfo
    {
        private SoyuzPatch attribute;
        private MethodBase[] targets;

        private MethodInfo prefix;
        private MethodInfo postfix;
        private MethodInfo transpiler;
        private MethodInfo finalizer;
        private MethodBase prepare;

        private PatchType patchType;

        public bool IsValid => attribute != null && targets.All(t => t != null);

        public SoyuzPatchInfo(Type type)
        {
            attribute = type.TryGetAttribute<SoyuzPatch>();
            patchType = attribute.patchType;
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
                if (Finder.debug) Log.Message(
                    $"SOYUZ: Prepare failed for {attribute.targetType.Name ?? null}:{attribute.targetMethod ?? null}");
                return;
            }

            foreach (var target in targets.ToHashSet())
            {
                if (target == null || target.IsAbstract || !target.HasMethodBody())
                {
                    if (Finder.debug) Log.Warning($"SOYUZ:[NOTANERROR] patching {target?.DeclaringType?.Name}:{target} is not possible! Patch attempt skipped!");
                    continue;
                }
                try
                {
                    harmony.Patch(target,
                        prefix: prefix != null ? new HarmonyMethod(prefix) : null,
                        postfix: postfix != null ? new HarmonyMethod(postfix) : null,
                        transpiler: transpiler != null ? new HarmonyMethod(transpiler) : null,
                        finalizer: finalizer != null ? new HarmonyMethod(finalizer) : null);
                    if (Finder.debug) Log.Message($"SOYUZ: Patched {target.DeclaringType.Name}:{target}");
                }
                catch (Exception er)
                {
                    Log.Warning($"SOYUZ: patching {target.DeclaringType.Name}:{target} is not possible!");
                }
            }
        }

        public void Unpatch(Harmony harmony)
        {
            foreach (var target in targets.ToHashSet())
            {
                try
                {
                    harmony.Unpatch(target, HarmonyPatchType.All, Finder.HarmonyID + ".Soyuz");
                }
                catch (Exception er)
                {
                    if (Finder.debug) Log.Warning($"SOYUZ: Unpatching {target.DeclaringType.Name}:{target} is not possible!");
                }
            }
        }
    }

    public class SoyuzPatcher
    {
        public static SoyuzPatchInfo[] patches = null;

        private readonly static Harmony harmony = new Harmony(Finder.HarmonyID + ".Soyuz");

        [Main.OnDefsLoaded]
        public static void PatchAll()
        {
            foreach (var patch in patches)
                patch.Patch(harmony);
            Log.Message($"SOYUZ: Patching finished");
            Finder.soyuzLoaded = true;
        }

        [Main.OnInitialization]
        public static void Intialize()
        {
            var flaggedTypes = GetSoyuzPatches();
            var patchList = new List<SoyuzPatchInfo>();
            foreach (var type in flaggedTypes)
            {
                var patch = new SoyuzPatchInfo(type);
                patchList.Add(patch);
                if (Finder.debug) Log.Message($"SOYUZ: found patch in {type} and is {(patch.IsValid ? "valid" : "invalid") }");
            }
            patches = patchList.Where(p => p.IsValid).ToArray();
        }

        private static IEnumerable<Type> GetSoyuzPatches()
        {
            return typeof(SoyuzPatcher).Assembly.GetTypes().Where(
                t => t.HasAttribute<SoyuzPatch>()
                );
        }
    }
}