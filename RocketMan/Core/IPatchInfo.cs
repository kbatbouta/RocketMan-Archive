using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    public abstract class IPatchInfo<T> where T : IPatch
    {
        private bool patchedSuccessfully;
        private T attribute;
        private MethodBase[] targets;
        private Type declaringType;

        private MethodInfo prefix;
        private MethodInfo postfix;
        private MethodInfo transpiler;
        private MethodInfo finalizer;
        private MethodBase prepare;
        private MethodBase replacement;

        private PatchType patchType;

        public abstract string PluginName { get; }
        public abstract string PatchTypeUniqueIdentifier { get; }

        public bool IsValid => attribute != null && targets.All(t => t != null);
        public bool PatchedSuccessfully
        {
            get => patchedSuccessfully;
        }
        public MethodBase ReplacementMethod
        {
            get => replacement;
        }
        public Type DeclaringType
        {
            get => declaringType;
        }

        public IPatchInfo(Type type)
        {
            declaringType = type;
            attribute = type.TryGetAttribute<T>();
            patchType = attribute.patchType;
            try
            {
                // TODO make this better and test it
                // if (attribute.GetType().Name != PatchTypeUniqueIdentifier)
                //    throw new InvalidOperationException($"{PluginName}: Mismatched PatchTypeUniqueIdentifier for {type}");
                if (patchType == PatchType.normal)
                {
                    if (attribute.methodType == MethodType.Getter)
                        targets = new MethodBase[1]
                            {
                                AccessTools.PropertyGetter(attribute.targetType, attribute.targetMethod)
                            };
                    else if (attribute.methodType == MethodType.Setter)
                        targets = new MethodBase[1]
                            {
                                AccessTools.PropertySetter(attribute.targetType, attribute.targetMethod)
                            };
                    else if (attribute.methodType == MethodType.Normal)
                        targets = new MethodBase[1]
                        {
                            AccessTools.Method(attribute.targetType, attribute.targetMethod, attribute.parameters,
                                attribute.generics)
                        };
                    else if (attribute.methodType == MethodType.Constructor)
                        targets = new MethodBase[1]
                       {
                            AccessTools.Constructor(attribute.targetType, attribute.parameters)
                       };
                    else
                        throw new Exception("Not implemented!");

                }
                else if (patchType == PatchType.empty)
                {
                    if (type.GetMethod("TargetMethods") != null)
                        targets = (type.GetMethod("TargetMethods").Invoke(null, null) as IEnumerable<MethodBase>).ToArray();
                    else
                        targets = (type.GetMethod("TargetMethod").Invoke(null, null) as IEnumerable<MethodBase>).ToArray();
                }
                prepare = declaringType.GetMethod("Prepare");
                prefix = declaringType.GetMethod("Prefix");
                postfix = declaringType.GetMethod("Postfix");
                transpiler = declaringType.GetMethod("Transpiler");
                finalizer = declaringType.GetMethod("Finalizer");
            }
            catch (Exception er)
            {
                Log.Error($"{PluginName}: target type {type.Name}:{er}");
                throw er;
            }
        }

        public virtual void Patch(Harmony harmony)
        {
            if (prepare != null && !((bool)prepare.Invoke(null, null)))
            {
                if (RocketDebugPrefs.debug) Log.Message($"{PluginName}: Prepare failed for {attribute.targetType.Name ?? null}:{attribute.targetMethod ?? null}");
                return;
            }
            foreach (var target in targets.ToHashSet())
            {
                if (!target.IsValidTarget())
                {
                    if (RocketDebugPrefs.debug) Log.Warning($"{PluginName}:[NOTANERROR] patching {target?.DeclaringType?.Name}:{target} is not possible! Patch attempt skipped");
                    continue;
                }
                try
                {
                    replacement = harmony.Patch(target,
                        prefix: prefix != null ? new HarmonyMethod(prefix) : null,
                        postfix: postfix != null ? new HarmonyMethod(postfix) : null,
                        transpiler: transpiler != null ? new HarmonyMethod(transpiler) : null,
                        finalizer: finalizer != null ? new HarmonyMethod(finalizer) : null);
                    if (RocketDebugPrefs.debug) Log.Message($"{PluginName}:[NOTANERROR] patching {target?.DeclaringType?.Name}:{target} finished!");
                    patchedSuccessfully = true;
                    OnPatchingSuccessful(replacement);
                }
                catch (Exception er)
                {
                    OnPatchingFailed(er);
                    Log.Warning($"{PluginName}:<color=orange>[ERROR]</color> <color=red>patching {target.DeclaringType.Name}:{target} Failed!</color> {er}");
                }
            }
        }

        public virtual void OnPatchingFailed(Exception er)
        {
        }

        public virtual void OnPatchingSuccessful(MethodBase replacement)
        {
        }
    }
}
