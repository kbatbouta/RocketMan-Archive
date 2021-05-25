using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RocketMan
{
    public partial class RocketMod : Mod
    {
        private static RocketModSettings settings;

        private static readonly Listing_Standard standard = new Listing_Standard();

        private static List<StatSettings> statsSettings = new List<StatSettings>();

        private static List<DilationSettings> dilationSettings = new List<DilationSettings>();

        private static string searchString = "";

        private const string PluginDir = "Plugins";

        private static int frameCounter = 0;

        public static RocketMod instance;

        public static Vector2 scrollPositionStatSettings = Vector2.zero;

        public RocketMod(ModContentPack content) : base(content)
        {
            Finder.Mod = this;
            Finder.ModContentPack = content;
            try
            {
                if (RocketEnvironmentInfo.IsDevEnv)
                {
                    Log.Warning("ROCKETMAN: YOU ARE LOADING AN EXPERIMENTAL PLUGIN!");
                    LoadPlugins(content, "Gagarin.dll", "Gagarin");
                }
                LoadPlugins(content, "Soyuz.dll", "Soyuz");
                LoadPlugins(content, "Proton.dll", "Proton");
                LoadPlugins(content, "Rocketeer.dll", "Rocketeer");
                RocketAssembliesInfo.Assemblies.Add(content.assemblies.loadedAssemblies[0]);
            }
            catch (Exception er)
            {
                Log.Error($"ROCKETMAN: loading plugin failed {er.Message}:{er.StackTrace}");
            }
            finally
            {
                Main.ReloadActions();
                foreach (var action in Main.onInitialization)
                    action.Invoke();
                instance = this;
                settings = GetSettings<RocketModSettings>();
                UpdateExceptions();
            }
        }

        private static void LoadPlugins(ModContentPack content, string pluginAssemblyName, string name)
        {
            string pluginsPath = Path.Combine(content.RootDir, PluginDir);
            if (File.Exists(Path.Combine(pluginsPath, pluginAssemblyName)) &&
                !LoadedModManager.runningMods.Any(m => m.Name.Contains(name)))
            {
                Log.Message($"{Path.Combine(pluginsPath, pluginAssemblyName)}");
                byte[] rawAssembly = File.ReadAllBytes(Path.Combine(pluginsPath, pluginAssemblyName));

                Assembly asm;
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                if (assemblies.All(a => a != null && a.GetName().Name != name))
                {
                    asm = AppDomain.CurrentDomain.Load(rawAssembly);
                    Log.Message(asm.GetName().Name);
                }
                else
                {
                    asm = assemblies.First(a => a.GetName().Name == name);
                }
                if (content.assemblies.loadedAssemblies.Any(a => a.FullName == asm.FullName))
                {
                    return;
                }
                RocketAssembliesInfo.Assemblies.Add(asm);
                content.assemblies.loadedAssemblies.Add(asm);
            }
        }

        public override string SettingsCategory()
        {
            return "RocketMan";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            DoSettings(inRect);
        }

        public override void WriteSettings()
        {
            if (Finder.WarmingUp)
                return;
            else
            {
                UpdateStats();
                UpdateDilationDefs();
                UpdateExceptions();
                base.WriteSettings();
            }
        }

        public static void DoSettings(Rect inRect, bool doStats = true, Action<Listing_Standard> extras = null)
        {
            ReadStats();
            ReadDilationSettings();

            var font = Text.Font;
            var anchor = Text.Anchor;
            var style = Text.CurFontStyle.fontStyle;
            var rect = inRect;
            standard.Begin(rect);
            Text.Font = GameFont.Tiny;
            Text.CurFontStyle.fontStyle = FontStyle.Normal;
            bool enabled = Finder.enabled;
            standard.CheckboxLabeled("Enabled", ref Finder.enabled);
            bool mainButtonToggle = Finder.mainButtonToggle;
            standard.CheckboxLabeled("Show RocketMan button/icon", ref Finder.mainButtonToggle,
                    "Due to some limiations some options aren't available from the game menu settings.");
            if (Finder.mainButtonToggle != mainButtonToggle)
            {
                MainButtonDef mainButton_WindowDef = DefDatabase<MainButtonDef>.GetNamed("RocketWindow", errorOnFail: false);
                if (mainButton_WindowDef != null)
                {
                    mainButton_WindowDef.buttonVisible = Finder.mainButtonToggle;
                    string state = Finder.mainButtonToggle ? "shown" : "hidden";
                    Log.Message($"ROCKETMAN: <color=red>MainButton</color> is now {state}!");
                }
            }
            if (enabled != Finder.enabled && !Finder.enabled)
            {
                ResetRocketDebugPrefs();
            }
            if (Finder.enabled)
            {
                standard.CheckboxLabeled("Show warmup progress bar on startup", ref Finder.showWarmUpPopup,
                    "This will show a warmup progress bar when you load a new map.");
                standard.GapLine();
                Text.CurFontStyle.fontStyle = FontStyle.Bold;
                standard.Label("Junk removal");
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                standard.CheckboxLabeled("Enable automatic corpses removal", ref Finder.corpsesRemovalEnabled,
                    "This removes corpses that aren't in view for a while and that aren't near your base to avoid breaking the game balance.");
                standard.GapLine();
                Text.CurFontStyle.fontStyle = FontStyle.Bold;
                standard.Label("Stats cache settings");
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                standard.CheckboxLabeled("Adaptive mode", ref Finder.learning, "Only enable for 30 minutes.");
                standard.CheckboxLabeled("Enable gear stat caching", ref Finder.statGearCachingEnabled,
                    "Can cause bugs.");

                standard.GapLine();
                bool oldDebugging = RocketDebugPrefs.debug;
                standard.CheckboxLabeled("Enable debugging", ref RocketDebugPrefs.debug, "Only for advanced users and modders");
                if (oldDebugging != RocketDebugPrefs.debug && !RocketDebugPrefs.debug)
                {
                    ResetRocketDebugPrefs();
                }
                if (RocketDebugPrefs.debug)
                {
                    standard.GapLine();
                    Text.CurFontStyle.fontStyle = FontStyle.Bold;
                    standard.Label("Debugging options");
                    Text.CurFontStyle.fontStyle = FontStyle.Normal;
                    standard.CheckboxLabeled("Enable Stat Logging (Will kill performance)", ref RocketDebugPrefs.statLogging);
                    standard.CheckboxLabeled("Enable GlowGrid flashing", ref RocketDebugPrefs.drawGlowerUpdates);
                    standard.CheckboxLabeled("Enable GlowGrid refresh", ref Finder.enableGridRefresh);
                    standard.Gap();
                    if (standard.ButtonText("Disable debugging related stuff"))
                        ResetRocketDebugPrefs();
                }
            }
            standard.End();
            try { if (frameCounter++ % 5 == 0 && !Finder.WarmingUp) settings.Write(); }
            catch (Exception er) { Log.Warning($"ROCKETMAN:[NOTANERROR] Writing settings failed with error {er}"); }
            Text.Font = font;
            Text.Anchor = anchor;
            Text.CurFontStyle.fontStyle = style;
        }

        public static void ResetRocketDebugPrefs()
        {
            RocketDebugPrefs.debug = false;
            RocketDebugPrefs.debug150MTPS = false;
            RocketDebugPrefs.logData = false;
            RocketDebugPrefs.statLogging = false;
            RocketDebugPrefs.flashDilatedPawns = false;
            RocketDebugPrefs.alwaysDilating = false;
            Finder.enableGridRefresh = false;
            Finder.refreshGrid = false;
            RocketDebugPrefs.singleTickIncrement = false;
        }

        public static void DoStatSettings(Rect rect)
        {
            UpdateStats();
            var counter = 0;
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Small;
            searchString = Widgets
                .TextArea(rect.TopPartPixels(25), searchString)
                .ToLower();
            rect.yMin += 35;
            Widgets.DrawMenuSection(rect);
            rect.yMax -= 5;
            rect.xMax -= 5;
            Widgets.BeginScrollView(rect.ContractedBy(1), ref scrollPositionStatSettings,
                new Rect(Vector2.zero, new Vector2(rect.width - 15, statsSettings.Count * 54)));
            Text.Font = GameFont.Tiny;
            Vector2 size = new Vector2(rect.width - 20, 54);
            Rect curRect = new Rect(new Vector2(2, 2), size);
            foreach (var settings in statsSettings)
            {
                if (searchString.Trim() == "" || settings.stat.ToLower().Contains(searchString))
                {
                    Rect rowRect = curRect.ContractedBy(5);
                    Text.Font = GameFont.Tiny;
                    Text.Anchor = TextAnchor.MiddleLeft;
                    if (counter % 2 == 0)
                        Widgets.DrawBoxSolid(curRect, new Color(0.2f, 0.2f, 0.2f));
                    Widgets.DrawHighlightIfMouseover(curRect);
                    Widgets.Label(rowRect.TopHalf(), string.Format("{0}. {1} set to expire in {2} ticks", counter++,
                        settings.stat,
                        settings.expireAfter));
                    settings.expireAfter =
                        (byte)Widgets.HorizontalSlider(rowRect.BottomHalf(), settings.expireAfter, 0, 255);
                    curRect.y += size.y;
                }
            }
            Widgets.EndScrollView();
            Text.Font = font;
            Text.Anchor = anchor;

            foreach (var setting in statsSettings)
                Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index] = (byte)setting.expireAfter;

            if (!Finder.WarmingUp && (WarmUpMapComponent.current?.Finished ?? true))
            {
                instance.WriteSettings();
                UpdateExceptions();
            }
        }

        public static void ReadStats()
        {
            if (statsSettings == null || statsSettings.Count == 0) return;

            foreach (var setting in statsSettings)
                setting.expireAfter = Finder.statExpiry[DefDatabase<StatDef>.defsByName[setting.stat].index];
        }

        public static void ReadDilationSettings()
        {
            if (dilationSettings == null || dilationSettings.Count == 0) return;

            foreach (var setting in dilationSettings)
            {
                if (DefDatabase<ThingDef>.defsByName.TryGetValue(setting.def, out var td))
                    setting.dilated = Finder.dilatedDefs[td.index];
                else
                    Log.Warning("ROCKETMAN: Failed to find stat upon reloading!");
            }
        }

        [Main.OnDefsLoaded]
        public static void UpdateDilationDefs()
        {
            if (dilationSettings == null) dilationSettings = new List<DilationSettings>();
            var failed = false;
            var defs = DefDatabase<ThingDef>.AllDefs.Where(
                d => d.race != null).ToList();
            if (statsSettings.Count != defs.Count())
            {
                dilationSettings.Clear();
                foreach (var def in defs)
                    dilationSettings.Add(new DilationSettings()
                    {
                        def = def.defName,
                        dilated = def.race.Animal && !def.race.IsMechanoid && !def.race.Humanlike
                    });
            }

            foreach (var setting in dilationSettings)
            {
                if (setting?.def != null && DefDatabase<ThingDef>.defsByName.TryGetValue(setting.def, out ThingDef def))
                    Finder.dilatedDefs[def.index] = setting.dilated;
                else
                {
                    failed = true;
                    break;
                }
            }
            if (failed)
            {
                Log.Warning("SOYUZ: Failed to reindex the ThingDef database");
                statsSettings.Clear();

                UpdateStats();
            }
        }

        public static void Reset()
        {
            var defs = DefDatabase<StatDef>.AllDefs;
            statsSettings.Clear();
            foreach (var def in defs)
                statsSettings.Add(new StatSettings
                { stat = def.defName, expireAfter = def.defName.PredictValueFromString() });
            var failed = false;
            foreach (var setting in statsSettings)
            {
                if (setting?.stat != null && DefDatabase<StatDef>.defsByName.TryGetValue(setting.stat, out StatDef def))
                    Finder.statExpiry[def.index] = (byte)setting.expireAfter;
                else
                {
                    failed = true;
                    break;
                }
            }
            if (failed)
            {
                Log.Warning("SOYUZ: Failed to reindex the statDef database");
                statsSettings.Clear();

                UpdateStats();
            }
            dilationSettings.Clear();
            UpdateDilationDefs();
            UpdateExceptions();
        }

        [Main.OnDefsLoaded]
        public static void UpdateStats()
        {
            if (statsSettings == null) statsSettings = new List<StatSettings>();

            var defs = DefDatabase<StatDef>.AllDefs;
            if (statsSettings.Count != defs.Count())
            {
                statsSettings.Clear();
                foreach (var def in defs)
                    statsSettings.Add(new StatSettings
                    { stat = def.defName, expireAfter = def.defName.PredictValueFromString() });
            }
            bool failed = false;
            foreach (StatSettings settings in statsSettings)
            {
                if (settings?.stat != null && DefDatabase<StatDef>.defsByName.TryGetValue(settings.stat, out StatDef def))
                    Finder.statExpiry[def.index] = (byte)settings.expireAfter;
                else
                {
                    failed = true;
                    break;
                }
            }
            if (failed)
            {
                Log.Warning("Failed to reindex the statDef database");
                statsSettings.Clear();

                UpdateStats();
            }

            UpdateExceptions();
        }

        public class StatSettings : IExposable
        {
            public int expireAfter;
            public string stat;

            public void ExposeData()
            {
                Scribe_Values.Look(ref stat, "statDef");
                Scribe_Values.Look(ref expireAfter, "expiryTime", 5);
            }
        }

        public class DilationSettings : IExposable
        {
            public bool dilated = true;
            public string def;

            public void ExposeData()
            {
                Scribe_Values.Look(ref def, "def");
                Scribe_Values.Look(ref dilated, "dilated");
            }
        }

        public class RocketModSettings : ModSettings
        {
            public override void ExposeData()
            {
                base.ExposeData();
                if (Scribe.mode == LoadSaveMode.Saving && Finder.WarmingUp && !(WarmUpMapComponent.current?.Finished ?? true)) WarmUpMapComponent.current.AbortWarmUp();
                if (Scribe.mode == LoadSaveMode.LoadingVars) ReadStats();
                Scribe_Values.Look(ref Finder.enabled, "enabled", true);
                Scribe_Values.Look(ref Finder.statGearCachingEnabled, "statGearCachingEnabled", true);
                Scribe_Values.Look(ref Finder.learning, "learning");
                Scribe_Values.Look(ref RocketDebugPrefs.debug, "debug", false);
                Scribe_Values.Look(ref Finder.showWarmUpPopup, "showWarmUpPopup", true);
                Scribe_Values.Look(ref Finder.alertThrottling, "alertThrottling", true);
                Scribe_Values.Look(ref Finder.disableAllAlert, "disableAllAlert", false);
                Scribe_Values.Look(ref Finder.timeDilation, "timeDilation", true);
                Scribe_Values.Look(ref Finder.timeDilationCaravans, "timeDilationCaravans", false);
                Scribe_Values.Look(ref Finder.timeDilationVisitors, "timeDilationVisitors", false);
                Scribe_Values.Look(ref Finder.timeDilationWorldPawns, "timeDilationWorldPawns", true);
                Scribe_Values.Look(ref Finder.timeDilationColonyAnimals, "timeDialationColonyAnimals", true);
                Scribe_Values.Look(ref Finder.timeDilationCriticalHediffs, "timeDilationCriticalHediffs", true);
                Scribe_Values.Look(ref Finder.ageOfGetValueUnfinalizedCache, "ageOfGetValueUnfinalizedCache");
                Scribe_Values.Look(ref Finder.universalCacheAge, "universalCacheAge");
                Scribe_Values.Look(ref Finder.mainButtonToggle, "mainButtonToggle", true);
                Scribe_Values.Look(ref Finder.corpsesRemovalEnabled, "corpsesRemovalEnabled", false);
                Scribe_Collections.Look(ref statsSettings, "statsSettings", LookMode.Deep);
                Scribe_Collections.Look(ref dilationSettings, "dilationSettings", LookMode.Deep);

                foreach (var action in Main.onScribe)
                    action.Invoke();
                UpdateExceptions();
            }
        }
    }
}