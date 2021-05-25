using System;
using System.Xml;
using Verse;
using RocketMan;
using System.Reflection;
using HarmonyLib;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Gagarin
{
    public static class LoadedLanguage_Patch
    {
        public static bool failed = false;
        public static bool cacheExists = false;

        public static string cachePath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache");
        public static string unifiedXmlPath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache/unified.xml");
        public static string unifiedXmlTempPath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache/unified.tmp.xml");

        public static XmlDocument document = null;

        public static GagarinPatchInfo[] patches = new GagarinPatchInfo[] {
                new GagarinPatchInfo(typeof(LoadedLanguage_LoadModXML_Patch)),
                new GagarinPatchInfo(typeof(LoadedLanguage_ApplyPatches_Patch)),
                new GagarinPatchInfo(typeof(LoadedLanguage_CombineIntoUnifiedXML_Patch)),
                new GagarinPatchInfo(typeof(LoadedLanguage_ParseAndProcessXML_Patch)),
                new GagarinPatchInfo(typeof(LoadedLanguage_ClearCachedPatches_Patch)),
                new GagarinPatchInfo(typeof(TKeySystem_Parse_Patch)),
            };

        [Main.OnInitialization]
        public static void Start()
        {
            PatchAll();
            if (failed)
            {
                Log.Warning($"GAGARIN: Stopped!");
                return;
            }
            if (!Directory.Exists(cachePath) || !File.Exists(unifiedXmlPath))
            {
                Log.Warning($"GAGARIN: Cache not found starting the caching process!");
                cacheExists = false;
                return;
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
                    failed = true;
                    break;
                }
            }
        }

        private static void DumpUnifiedPatchedDocument(XmlDocument originalXmlDoc)
        {
            string tempFile = unifiedXmlTempPath;
            if (File.Exists(tempFile))
                File.Delete(tempFile);
            originalXmlDoc.Save(tempFile);

            XmlDocument document = new XmlDocument();
            document.Load(tempFile);
            //XmlNode node;
            //XmlNodeList nodes = document.DocumentElement.ChildNodes;
            //for (int i = 0; i < nodes.Count; i++)
            //{
            //    node = nodes[i];
            //    node.GetXPath();
            //}
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.LoadModXML))]
        public static class LoadedLanguage_LoadModXML_Patch
        {
            public static bool Prepare() => !failed;

            public static bool Prefix()
            {
                if (failed || !cacheExists)
                    return true;
                return true;
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.CombineIntoUnifiedXML))]
        public static class LoadedLanguage_CombineIntoUnifiedXML_Patch
        {
            public static bool Prepare() => !failed;

            public static bool Prefix()
            {
                if (failed || !cacheExists)
                    return true;
                return false;
            }
        }

        [GagarinPatch(typeof(TKeySystem), nameof(TKeySystem.Parse))]
        public static class TKeySystem_Parse_Patch
        {
            public static bool Prepare() => !failed;

            public static bool Prefix()
            {
                if (failed || !cacheExists)
                    return true;
                return true;
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ApplyPatches))]
        public static class LoadedLanguage_ApplyPatches_Patch
        {
            public static bool Prepare() => !failed;

            public static bool Prefix(XmlDocument xmlDoc, out bool __state)
            {
                __state = true;
                if (failed || cacheExists)
                    return false;
                if (!Directory.Exists(cachePath))
                    Directory.CreateDirectory(cachePath);
                if (File.Exists(unifiedXmlPath))
                    File.Delete(unifiedXmlPath);
                __state = false;
                return true;
            }

            public static void Postfix(XmlDocument xmlDoc, bool __state)
            {
                if (__state)
                {
                    return;
                }
                DumpUnifiedPatchedDocument(xmlDoc);
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ParseAndProcessXML))]
        public static class LoadedLanguage_ParseAndProcessXML_Patch
        {
            public static bool Prepare() => !failed;

            public static bool Prefix()
            {
                if (failed || !cacheExists)
                    return true;
                return true;
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ClearCachedPatches))]
        public static class LoadedLanguage_ClearCachedPatches_Patch
        {
            public static bool Prepare() => !failed;

            public static bool Prefix()
            {
                if (failed || !cacheExists)
                    return true;
                return true;
            }
        }
    }
}
