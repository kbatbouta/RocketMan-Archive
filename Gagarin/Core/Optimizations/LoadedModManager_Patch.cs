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
using System.Linq;

namespace Gagarin
{
    public static class LoadedModManager_Patch
    {
        private const string _SECRET = "eyOvT6fExIMGr1TXQZ4J9I5mA6i7LUXLMS19sllSUAIF0VKvLCeYztC8ikAb";

        public static bool failed = false;
        public static bool cacheExists = false;
        public static bool usedCache = false;
        public static bool finished = false;

        public static string cachePath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache");
        public static string unifiedXmlPath = Path.Combine(GenFilePaths.ConfigFolderPath, "Cache/unified.xml");

        public static XmlDocument document = null;

        public static GagarinPatchInfo[] patches = new GagarinPatchInfo[] {
            new GagarinPatchInfo(typeof(LoadModXML_Patch)),
            new GagarinPatchInfo(typeof(CombineIntoUnifiedXML_Patch)),
            new GagarinPatchInfo(typeof(ApplyPatches_Patch)),
            new GagarinPatchInfo(typeof(ParseAndProcessXML_Patch)),
            new GagarinPatchInfo(typeof(TKeySystem_Parse_Patch)),
        };

        [Main.OnInitialization]
        public static void Start()
        {
            LoadedModManager_Patch.PatchAll();
            if (failed)
            {
                Log.Warning($"GAGARIN: Stopped!");
                finished = true;
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
        public static class LoadModXML_Patch
        {
            private static Stopwatch stopwatch = new Stopwatch();

            public static bool Prefix()
            {
                Log.Message("GAGARIN: started LoadModXML");
                stopwatch.Start();
                if (finished || failed)
                    return true;
                bool skip = false;
                try
                {

                }
                catch (Exception er)
                {
                    Log.Error($"GAGARIN: Error in patch {er}");
                }
                return !skip;
            }

            public static void Postfix()
            {
                stopwatch.Stop();
                Log.Message($"GAGARIN:[<color=orange>PROFILING</color>] LoadModXML took " +
                    $"{Math.Round((float)stopwatch.ElapsedTicks / Stopwatch.Frequency, 4)} MS");
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.CombineIntoUnifiedXML))]
        public static class CombineIntoUnifiedXML_Patch
        {
            private static Stopwatch stopwatch = new Stopwatch();

            public static bool Prefix()
            {
                Log.Message("GAGARIN: started CombineIntoUnifiedXML");
                stopwatch.Start();
                if (finished || failed)
                    return true;
                bool skip = false;
                try
                {

                }
                catch (Exception er)
                {
                    Log.Error($"GAGARIN: Error in patch {er}");
                }
                return !skip;
            }

            public static void Postfix()
            {
                stopwatch.Stop();
                Log.Message($"GAGARIN:[<color=orange>PROFILING</color>] CombineIntoUnifiedXML took " +
                    $"{Math.Round((float)stopwatch.ElapsedTicks / Stopwatch.Frequency, 4)} MS");
                failed = true;
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ApplyPatches))]
        public static class ApplyPatches_Patch
        {
            private static Stopwatch stopwatch = new Stopwatch();

            public static bool Prefix(ref XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
            {
                Log.Message("GAGARIN: started ApplyPatches");
                stopwatch.Start();
                if (finished || failed)
                    return true;
                bool skip = false;
                try
                {
                    if (cacheExists)
                    {
                        xmlDoc = new XmlDocument();
                        xmlDoc.Load(unifiedXmlPath);
                        usedCache = true;
                        return false;
                    }
                }
                catch (Exception er)
                {
                    Log.Error($"GAGARIN: Error in patch {er}");
                    failed = true;
                }
                return !skip;
            }

            public static void Postfix(ref XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
            {
                stopwatch.Stop();
                Log.Message($"GAGARIN:[<color=orange>PROFILING</color>] ApplyPatches took " +
                    $"{Math.Round((float)stopwatch.ElapsedTicks / Stopwatch.Frequency, 4)} S");
                if (cacheExists || failed)
                    return;
                try
                {
                    xmlDoc.Save(unifiedXmlPath);
                }
                catch (Exception er)
                {
                    if (File.Exists(unifiedXmlPath))
                        File.Delete(unifiedXmlPath);
                    Log.Error($"GAGARIN: Loading cached data failed! {er}");
                    failed = true;
                }
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ParseAndProcessXML))]
        public static class ParseAndProcessXML_Patch
        {
            private static Stopwatch stopwatch = new Stopwatch();

            public static bool Prefix()
            {
                Log.Message("GAGARIN: started ParseAndProcessXML");
                stopwatch.Start();
                if (finished || failed)
                    return true;
                bool skip = false;
                try
                {

                }
                catch (Exception er)
                {
                    Log.Error($"GAGARIN: Error in patch {er}");
                    failed = true;
                }
                return !skip;
            }

            public static void Postfix()
            {
                stopwatch.Stop();
                Log.Message($"GAGARIN:[<color=orange>PROFILING</color>] ParseAndProcessXML took " +
                    $"{Math.Round((float)stopwatch.ElapsedTicks / Stopwatch.Frequency, 4)} S");
            }
        }

        [GagarinPatch(typeof(TKeySystem), nameof(TKeySystem.Parse))]
        public static class TKeySystem_Parse_Patch
        {
            private static Stopwatch stopwatch = new Stopwatch();

            public static bool Prefix()
            {
                Log.Message("GAGARIN: started TKeySystem.Parse");
                stopwatch.Start();
                if (finished || failed)
                    return true;
                bool skip = false;
                try
                {

                }
                catch (Exception er)
                {
                    Log.Error($"GAGARIN: Error in patch {er}");
                    failed = true;
                }
                return !skip;
            }

            public static void Postfix()
            {
                stopwatch.Stop();
                Log.Message($"GAGARIN:[<color=orange>PROFILING</color>] TKeySystem.Parse took " +
                    $"{Math.Round((float)stopwatch.ElapsedTicks / Stopwatch.Frequency, 4)} S");
            }
        }

        [GagarinPatch(typeof(LoadedModManager), nameof(LoadedModManager.ClearCachedPatches))]
        public static class ClearCachedPatches_Patch
        {
            private static Stopwatch stopwatch = new Stopwatch();

            public static bool Prefix()
            {
                Log.Message("GAGARIN: started ClearCachedPatches");
                stopwatch.Start();
                if (finished || failed)
                    return true;
                bool skip = false;
                try
                {
                    if (usedCache)
                    {
                        return false;
                    }
                }
                catch (Exception er)
                {
                    Log.Error($"GAGARIN: Error in patch {er}");
                    failed = true;
                }
                finally { finished = true; }
                return !skip;
            }

            public static void Postfix()
            {
                stopwatch.Stop();
                Log.Message($"GAGARIN:[<color=orange>PROFILING</color>] ClearCachedPatches took " +
                    $"{Math.Round((float)stopwatch.ElapsedTicks / Stopwatch.Frequency, 4)} MS");
            }
        }
    }
}
