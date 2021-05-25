using System;
using System.Xml;
using Verse;
using RocketMan;
using System.Reflection;
using HarmonyLib;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Gagarin
{
    public static class LoadedLanguage_Patch
    {
        private const string _SECRET = "eyOvT6fExIMGr1TXQZ4J9I5mA6i7LUXLMS19sllSUAIF0VKvLCeYztC8ikAb";

        public static bool failed = false;
        public static bool cacheExists = false;

        public static string cachePath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache");
        public static string unifiedXmlPath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache/unified.xml");
        public static string unifiedXmlTempPath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache/unified.tmp.xml");

        public static XmlDocument document = null;

        public static GagarinPatchInfo[] patches = new GagarinPatchInfo[] {
                new GagarinPatchInfo(typeof(LoadedLanguage_ApplyPatches_Patch)),
                //new GagarinPatchInfo(typeof(LoadedLanguage_CombineIntoUnifiedXML_Patch)),
                new GagarinPatchInfo(typeof(LoadedLanguage_LoadModXML_Patch))
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
            cacheExists = true;

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

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.LoadModXML))]
        public static class LoadedLanguage_LoadModXML_Patch
        {
            public static bool Prepare() => !failed;

            public static bool Prefix(ref LoadableXmlAsset[] __result)
            {
                if (!cacheExists || failed)
                    return true;
                __result = new LoadableXmlAsset[] {
                    new LoadableXmlAsset(_SECRET, _SECRET, _SECRET) { mod = Finder.ModContentPack }
                };
                return false;
            }
        }

        [GagarinPatch(typeof(LoadableXmlAsset), methodType = MethodType.Constructor)]
        public static class LoadableXmlAsset_Constructor_Patch
        {
            public static bool Prepare() => !failed;

            public static bool Prefix(LoadableXmlAsset __instance, string name, string fullFolderPath, string contents)
            {
                if (!cacheExists || failed)
                    return true;
                if (false
                    || _SECRET != name
                    || _SECRET != fullFolderPath
                    || _SECRET != contents)
                    return true;
                contents = File.ReadAllText(unifiedXmlPath);
                __instance.name = "unified.xml";
                __instance.fullFolderPath = cachePath;
                try
                {
                    XmlReaderSettings settings = new XmlReaderSettings
                    {
                        IgnoreComments = true,
                        IgnoreWhitespace = true,
                        CheckCharacters = false
                    };
                    StringReader input = new StringReader(contents);
                    XmlReader xmlReader = XmlReader.Create(input, settings);
                    __instance.xmlDoc = new XmlDocument();
                    __instance.xmlDoc.Load(xmlReader);
                    document = __instance.xmlDoc;
                }
                catch (Exception ex)
                {
                    Log.Warning("Exception reading " + name + " as XML: " + ex);
                    __instance.xmlDoc = null;
                }
                return false;
            }
        }

        //[GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.CombineIntoUnifiedXML))]
        //public static class LoadedLanguage_CombineIntoUnifiedXML_Patch
        //{
        //    public static bool Prepare() => !failed;

        //    public static bool Prefix(ref XmlDocument __result)
        //    {
        //        if (failed || !cacheExists) return true;
        //        __result = document;
        //        return false;
        //    }
        //}

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ClearCachedPatches))]
        public static class LoadedLanguage_ClearCachedPatches_Patch
        {
            public static bool Prepare() => !failed;

            public static bool Prefix()
            {
                if (failed || !cacheExists) return true;
                return false;
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ApplyPatches))]
        public static class LoadedLanguage_ApplyPatches_Patch
        {
            public static bool Prepare() => !failed;

            public static bool Prefix(XmlDocument xmlDoc, out bool __state)
            {
                __state = true;
                if (failed)
                    return true;
                if (cacheExists)
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
                xmlDoc.Save(unifiedXmlPath);
            }
        }
    }
}
