using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using HarmonyLib;
using RimWorld.QuestGen;
using RocketMan;
using Verse;

namespace Gagarin
{
    public static class PatchOperation_Patch
    {
        public static int PatchCounter = 0;

        public static bool cacheExists = false;
        public static bool cacheUsed = false;

        public static string cachePath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache");
        public static string cachedUnifiedXmlPath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache/unified.xml");

        public static GagarinPatchInfo[] patches = new GagarinPatchInfo[] {
            new GagarinPatchInfo(typeof(ApplyWorker_Patch)),
            new GagarinPatchInfo(typeof(ApplyPatches_Patch)),
        };

        [Main.OnInitialization]
        public static void Start()
        {
            PatchOperation_Patch.PatchAll();
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
                Log.Message($"GAGARIN: Created cache folder at {cachedUnifiedXmlPath}");
            }
            if (!File.Exists(cachedUnifiedXmlPath))
            {
                Log.Warning($"GAGARIN: Cache not found starting the caching process!");
                cacheExists = false;
                return;
            }
            cacheExists = true;
            Log.Warning($"GAGARIN: Cache <color=green>FOUND</color>!");
        }

        public static bool TryGetCachedXmlNodeList(string xpath, Type patchType, out XmlNodeList nodeList)
        {
            nodeList = null;
            return false;
        }

        public static string report = "";

        public static void CacheXmlNodeList(string xpath, Type patchType, XmlNodeList nodeList)
        {
            foreach (XmlNode node in nodeList)
            {
                report += node.GetXPath() + "\n";
            }
        }

        public static void Prepare(XmlDocument document, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
        {
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ApplyPatches))]
        public static class ApplyPatches_Patch
        {
            public static Stopwatch stopwatch = new Stopwatch();

            public static void Prefix(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
            {
                stopwatch.Start();
                Log.Message("GAGARIN: Patching started");
                Prepare(xmlDoc, assetlookup);
            }

            public static void Postfix()
            {
                stopwatch.Stop();
                Log.Message($"GAGARIN: <color=orange>Patching finihed!</color> and took <color=red>{Math.Round((float)stopwatch.ElapsedTicks / Stopwatch.Frequency, 3)}</color>");
                File.WriteAllText(cachedUnifiedXmlPath, report);
            }
        }

        [GagarinPatch]
        public static class ApplyWorker_Patch
        {
            private static FieldInfo fxpath = AccessTools.Field(typeof(PatchOperationPathed), nameof(PatchOperationPathed.xpath));

            private static MethodBase mSelectNodes = AccessTools.Method(typeof(XmlNode), nameof(XmlNode.SelectNodes), parameters: new[] { typeof(string) });

            private static MethodBase mGetEnumerator = AccessTools.Method(typeof(XmlNodeList), nameof(XmlNodeList.GetEnumerator));

            private static MethodBase mHijacked_SelectNodes = AccessTools.Method(typeof(ApplyWorker_Patch), nameof(ApplyWorker_Patch.Hijacked_SelectNodes));

            public static IEnumerable<MethodBase> TargetMethods()
            {
                foreach (Type patchType in typeof(PatchOperation).AllSubclassesNonAbstract())
                {
                    if (patchType.IsAbstract)
                        continue;
                    MethodBase mApplyWorker = AccessTools.Method(patchType, nameof(PatchOperation.ApplyWorker));
                    if (!mApplyWorker.IsValidTarget())
                        continue;
                    if (!mApplyWorker.HasMethodBody())
                        continue;
                    Log.Message($"mApplyWorker");
                    yield return mApplyWorker;
                }
            }

            public static void Prefix(PatchOperation __instance, XmlDocument xml)
            {
                PatchCounter++;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase originalMethod)
            {
                List<CodeInstruction> codes = instructions.ToList();
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].OperandIs(mSelectNodes))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, mHijacked_SelectNodes);
                        continue;
                    }
                    yield return codes[i];
                }
            }

            private static XmlNodeList Hijacked_SelectNodes(XmlDocument document, string xpath, PatchOperation operation)
            {
                Type operationType = operation.GetType();
                if (TryGetCachedXmlNodeList(xpath, operationType, out XmlNodeList nodes))
                    return nodes;
                nodes = document.SelectNodes(xpath);
                CacheXmlNodeList(xpath, operationType, nodes);
                return nodes;
            }
        }

        public static void PatchAll()
        {
            for (int i = 0; i < patches.Length; i++)
            {
                try { patches[i].Patch(GagarinPatcher.harmony); }
                catch (Exception er)
                {
                    Log.Error($"GAGARIN: LoadedLanguage_Patch PATCHING FAILED! {patches[i].DeclaringType}:{er}");
                    break;
                }
            }
        }
    }
}
