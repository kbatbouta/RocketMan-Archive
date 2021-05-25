using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RocketMan;
using Verse;

namespace Gagarin
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class GagarinPatch : IPatch
    {
        public GagarinPatch()
        {
        }

        public GagarinPatch(Type targetType, string targetMethod = null, MethodType methodType = MethodType.Normal, Type[] parameters = null, Type[] generics = null) : base(targetType, targetMethod, methodType, parameters, generics)
        {
        }
    }

    public class GagarinPatchInfo : IPatchInfo<GagarinPatch>
    {
        public override string PluginName => "Gagarin";
        public override string PatchTypeUniqueIdentifier => nameof(GagarinPatch);

        public GagarinPatchInfo(Type type) : base(type)
        {
        }
    }

    public class GagarinPatcher
    {
        public static GagarinPatchInfo[] patches = null;

        private readonly static Harmony harmony = new Harmony(Finder.HarmonyID + ".Gagarin");

        [Main.OnInitialization]
        public static void Initialize()
        {
            if (!RocketEnvironmentInfo.IsDevEnv)
            {
                Log.Error($"GARAGIN:[ERROR] Attempted to start an experimental plugin!");
                throw new InvalidOperationException($"GARAGIN:[ERROR] NOT A DEV ENVIRONMENT");
            }
            IEnumerable<Type> flaggedTypes = GetPatches();
            List<GagarinPatchInfo> patchList = new List<GagarinPatchInfo>();
            foreach (Type type in flaggedTypes)
            {
                GagarinPatchInfo patch = new GagarinPatchInfo(type);
                patchList.Add(patch);
                if (RocketDebugPrefs.debug)
                {
                    Log.Message($"GAGARIN: found patch in {type} and is {(patch.IsValid ? "valid" : "invalid") }");
                }
            }
            patches = patchList.Where(p => p.IsValid).ToArray();
            foreach (var patch in patches)
            {
                patch.Patch(harmony);
            }
            Log.Message($"GAGARIN: Patching finished");
            Finder.gagarinLoaded = true;
        }

        private static IEnumerable<Type> GetPatches()
        {
            return typeof(GagarinPatcher).Assembly.GetLoadableTypes().Where(t => t.HasAttribute<GagarinPatch>());
        }
    }
}
